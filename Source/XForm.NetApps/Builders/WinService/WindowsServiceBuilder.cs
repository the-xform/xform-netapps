// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using XForm.NetApps.Builders.WinService.Interfaces;
using XForm.NetApps.Hosts.WinService;
using XForm.NetApps.Providers.File;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Builders.WinService
{
    public static class WindowsServiceBuilder
    {
		#region - Public Methods -

		/// <summary>
		/// Builds and returns a HostApplicationBuilder for a Windows Service application.
		/// </summary>
		/// <param name="hostBuilder"></param>
		/// <param name="windowsServiceOptions"></param>
		/// <returns></returns>
		public static HostApplicationBuilder CreateHostBuilder(WindowsServiceOptions windowsServiceOptions)
		{
			Xssert.IsNotNull(windowsServiceOptions.ServiceName, nameof(windowsServiceOptions.ServiceName));

			// Inject appsettings.json, config files, additional json configs,
			// and core services like IJsonUtilities, ISequentialGuidGenerator, and Serilog logger.
			var host_app_builder = CommonAppBuilder.CreateCommonApplicationBuilder(windowsServiceOptions.ServiceName, windowsServiceOptions.Args);

			#region - Configure Workers -

			// Configure workers.
			host_app_builder.Services.AddSingleton<IWorkerHostFactory, WorkerHostFactory>();
			
			// Gets all the workers from the "Workers" config. This allows us to actually have multiple workers defined in the same assembly
			// e.g. an actual worker that does some business operation and a companion worker to clean up the database table
			// related to the main business worker, at regular intervals.
			var worker_configs = host_app_builder.Configuration.GetSection("Workers").Get<IReadOnlyDictionary<string, WorkerConfig>>();
			Xssert.IsNotNull(worker_configs);

			var list_of_worker_assemblies = worker_configs.Select(w => w.Value.AssemblyPath).ToList();

			// Get the assembly paths for worker assemblies and create a composite file provider for them.
			// The composite file provider will then be used by the AssemblyProvider to resolve
			// assemblies from these paths.
			var worker_dependency_assembly_files_providers = list_of_worker_assemblies
											.Select(assemblyPath => Path.IsPathRooted(assemblyPath) == false
												? Path.Join(AppContext.BaseDirectory, assemblyPath)
												: assemblyPath)
											.Select(assemblyPath =>
											{
												return Path.GetDirectoryName(assemblyPath) ?? throw new Exception($"WorkerPlugins: Unable to get directory name for worker assembly path '{assemblyPath}'");
											}).Distinct().ToList().Select(af => new PhysicalFileWritableProvider(af)).ToList();

			var composite_file_provider = new CompositeFileProvider(worker_dependency_assembly_files_providers);

			// Add assembly provider for assembly resolution from all worker assembly paths
			//DoRegisterServiceAssemblyProviders(host_app_builder.Configuration, composite_file_provider);

			// Register assembly providers for assembly resolution.
			CommonAppBuilder.RegisterAssemblyProviders(host_app_builder, composite_file_provider);

			// Initialize the workers
			host_app_builder.Services.DoAddAndConfigureWorkers(host_app_builder);

			#endregion - Configure Workers -

			// Add a windows service
			host_app_builder.Services.AddWindowsService(wsOptions => wsOptions.ServiceName = windowsServiceOptions.ServiceName);

			windowsServiceOptions.ApplicationSetup(host_app_builder.Configuration, host_app_builder.Services);

			return (HostApplicationBuilder)host_app_builder;
		}

		/// <summary>
		/// Builds and returns IHost for a Windows Service application.
		/// </summary>
		/// <param name="windowsServiceOptions"></param>
		/// <returns></returns>
		public static IHost CreateHost(WindowsServiceOptions windowsServiceOptions)
		{
			return CreateHostBuilder(windowsServiceOptions).Build();
		}

		#endregion - Public Methods -

		#region - Private Methods -

        private static void DoAddAndConfigureWorkers(
            this IServiceCollection services,
            IHostApplicationBuilder hostBuilder)
        {
            // Get workers section
			var workers_configuration_section = hostBuilder.Configuration.GetSection("Workers");
			Xssert.IsNotNull(workers_configuration_section, nameof(workers_configuration_section));

			var all_workers = workers_configuration_section.Get<IReadOnlyDictionary<string, WorkerConfig>>();
            if (all_workers == null || all_workers.Any() == false)
            {
                return;
            }

            foreach (string key in all_workers.Keys)
            {
                var the_worker_configuration_section = workers_configuration_section.GetSection(key);
                var the_worker_config = the_worker_configuration_section.Get<WorkerConfig>(); // Workers:PingWorker

				Xssert.IsNotNull(the_worker_config, "worker_config");
				Xssert.IsNotNullOrEmpty(the_worker_config!.FactoryType, "worker_config.FactoryType");

				if (Path.IsPathRooted(the_worker_config.AssemblyPath) == false)
				{
					the_worker_config.AssemblyPath = Path.Join(AppContext.BaseDirectory, the_worker_config.AssemblyPath);
				}

				// Inject services needed by this worker
				var worker_services_injector_type = AssemblyLoadContext.Default.LoadFromAssemblyPath(the_worker_config.AssemblyPath).GetType(the_worker_config.WorkerServiceInjectorType);
				Xssert.IsNotNull(worker_services_injector_type, nameof(worker_services_injector_type));

				var worker_services_injector_instance = Activator.CreateInstance(worker_services_injector_type!) as IWorkerServiceInjector;
				Xssert.IsNotNull(worker_services_injector_instance, nameof(worker_services_injector_instance));

				// Setuo the configuration
				worker_services_injector_instance.ConfigureServicesForWorker(hostBuilder.Services, the_worker_configuration_section);

                services.AddSingleton<IHostedService, WorkerHost>(sp => sp.GetRequiredService<IWorkerHostFactory>().Create(the_worker_configuration_section, sp, the_worker_config.AssemblyPath, the_worker_config.FactoryType));
            }
        }

		#endregion - Private Methods -
	}
}
