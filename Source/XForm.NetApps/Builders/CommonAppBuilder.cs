// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using XForm.NetApps.Extensions;
using XForm.NetApps.Providers.Assembly;
using XForm.NetApps.Providers.File;
using XForm.Core.Interfaces;
using XForm.Utilities;
using XForm.Utilities.Validations;
using XForm.NetApps.Builders.WinService;

namespace XForm.NetApps.Builders;

/// <summary>
/// The common builder for all the specific builders like console app builder, web app builder etc, to create 
/// a common host application builder with core services and any additional services injected through implementation 
/// of IServicesInjector interface specified in the config file.
/// </summary>
public static class CommonAppBuilder
{
	#region - Public Methods -

	public static IHostApplicationBuilder CreateCommonApplicationBuilder(IHostApplicationBuilder hostApplicationBuilder, string applicationName, string[] args)
	{
		var host_application_builder_settings = new HostApplicationBuilderSettings
		{
			ApplicationName = applicationName,
			Args = args,
		};

		return DoConfigureBuilder(hostApplicationBuilder, host_application_builder_settings);
	}

	/// <summary>
	/// Creates a common application builder with injected core services like seri-logger, guid provider, and common services 
	/// requested to be injected through IServicesInjector implementation provided through config file.
	/// </summary>
	/// <param name="applicationName"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public static IHostApplicationBuilder CreateCommonApplicationBuilder(string applicationName, string[] args)
	{
		var host_application_builder_settings = new HostApplicationBuilderSettings
		{
			ApplicationName = applicationName,
			Args = args,
		};

		var builder = Host.CreateApplicationBuilder(host_application_builder_settings);

		return DoConfigureBuilder(builder, host_application_builder_settings);
	}

	/// <summary>
	/// Register any additional assembly providers.
	/// </summary>
	/// <param name="hostApplicationBuilder"></param>
	/// <param name="fileProvider"></param>
	public static void RegisterAssemblyProviders(IHostApplicationBuilder hostApplicationBuilder, IFileProvider? fileProvider = null)
	{
		var file_providers = new List<IFileProvider>();
		if (fileProvider != null)
		{
			file_providers.Add(fileProvider);
		}

		var assembly_provider_section = hostApplicationBuilder.Configuration.GetSection("AssemblyProvider");
		if (assembly_provider_section.Exists() == true)
		{
			var file_provider_sections = assembly_provider_section.GetSection("FileProviders").GetChildren();
			if (file_provider_sections.Any() == true)
			{
				foreach (var file_provider_section in file_provider_sections)
				{
					var file_provider_options = file_provider_section.Get<PhysicalFileProviderOptions>();
					Xssert.IsNotNull(file_provider_options, nameof(file_provider_options));

					var factory = InstanceCreatorHelper.InstantiateType<IFileProviderFactory>(
												file_provider_options.Factory,
												file_provider_section);
					file_providers.Add(factory.Create());
				}
			}
		}

		if (file_providers.Count == 0)
		{
			// Add the default file provider if none is configured.
			file_providers.Add(new PhysicalFileProvider(AppContext.BaseDirectory));
		}

		var assembly_provider = new AssemblyProvider(new AppDomainLoadedAssembliesProvider(), new AssemblyLoadContextAssemblyLoader(new CompositeFileProvider(file_providers)));

		AssemblyLoadContext.Default.Resolving += (context, name) => assembly_provider.Get(name);
	}

	#endregion - Public Methods -

	#region - Private Methods -

	private static IHostApplicationBuilder DoConfigureBuilder(IHostApplicationBuilder builder, HostApplicationBuilderSettings host_application_builder_settings)
	{
		builder.Configuration.AddCommandLine(host_application_builder_settings.Args ?? []);

		// Add appsettings file
		var app_settings_file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
		if (File.Exists(app_settings_file) == true)
		{
			builder.Configuration
				.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: false)
				.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
			//.AddCommandLine(args);
		}

		// Get any additional JSON config files provided via commandline args.
		var provided_addl_json_configs = (builder.Configuration.GetSection("AdditionalJsonConfigs").Get<IEnumerable<string>>() ?? Enumerable.Empty<string>()).ToList();
		foreach (var json_config_file in provided_addl_json_configs)
		{
			Xssert.IsNotNullOrEmpty(json_config_file, nameof(json_config_file));

			var json_config_file_path = string.Empty;
			if (Path.IsPathRooted(json_config_file))
			{
				json_config_file_path = json_config_file;
			}
			else
			{
				json_config_file_path = Path.Combine(LocationUtilities.GetEntryAssemblyDirectory(), json_config_file);
			}

			Xssert.FileExists(new PhysicalFileInfo(new FileInfo(json_config_file_path)), nameof(json_config_file_path));
			builder.Configuration.AddJsonFile(json_config_file_path, optional: false, reloadOnChange: false);
		}

		// Add core services - ISequentialGuidGenerator, IJsonUtilities, and Serilog.
		builder.Services.AddCoreServices(builder.Configuration);

		// Inject any external service if an injector is configured
		var service_injector_config = builder.Configuration.GetSection("ServiceInjector").Get<ServiceInjectorConfig>();
		Xssert.IsNotNull(service_injector_config);

		if (service_injector_config.IsEnabled == true)
		{
			var injector_assembly_path = service_injector_config.AssemblyPath;
			Xssert.IsNotNull(injector_assembly_path, nameof(injector_assembly_path));

			if (Path.IsPathRooted(injector_assembly_path) == false)
			{
				injector_assembly_path = Path.Join(AppContext.BaseDirectory, service_injector_config.AssemblyPath);
			}

			Xssert.FileExists(new PhysicalFileInfo(new FileInfo(injector_assembly_path)), nameof(injector_assembly_path));

			var common_services_injector_type = AssemblyLoadContext.Default.LoadFromAssemblyPath(injector_assembly_path).GetType(service_injector_config.TypeName);
			Xssert.IsNotNull(common_services_injector_type, nameof(common_services_injector_type));

			var common_services_injector_instance = Activator.CreateInstance(common_services_injector_type!) as IServicesInjector;
			Xssert.IsNotNull(common_services_injector_instance, nameof(common_services_injector_instance));

			// Setuo the configuration
			common_services_injector_instance.ConfigureConfiguration(builder.Configuration);
			common_services_injector_instance.ConfigureServices(builder.Configuration, builder.Services);
		}

		/* 
		 * NOTE: This is final point for injection of services in ServiceCollection and 
		 * no more service injection will work beyond this point because 
		 * hostApplicationBuilder.Services.BuildServiceProvider() will be 
		 * invoked after this point. 
		 */

		// Add any assembly providers if configured -
		// This must be done by the callers of this method as a separate call
		// because the caller may want to provide a custom file provider to be
		// used by the assembly provider.
		//RegisterAssemblyProviders(builder);

		// Return host
		return builder;
	}

	#endregion - Private Methods -
}
