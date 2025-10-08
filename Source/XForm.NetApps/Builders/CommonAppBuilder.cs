// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using XForm.Core.Interfaces;
using XForm.NetApps.Builders.WinService;
using XForm.NetApps.Extensions;
using XForm.NetApps.Providers.Assembly;
using XForm.NetApps.Providers.File;
using XForm.Utilities;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Builders;

/// <summary>
/// The common builder for all the specific builders like console app builder, web app builder etc, to create 
/// a common host application builder with core services and any additional services injected through implementation 
/// of IServicesInjector interface specified in the config file.
/// </summary>
public static class CommonAppBuilder
{
	#region - Public Methods -

	/// <summary>
	/// Creates a common host builder with injected core services like seri-logger, guid provider, and common services 
	/// requested to be injected through IServicesInjector implementation provided through config file.
	/// </summary>
	/// <param name="applicationName"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public static IHostBuilder CreateHostBuilder(string applicationName, string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args);

		builder.ConfigureHostConfiguration(configBuilder =>
		{
			configBuilder.AddCommandLine(args);

			// Add appsettings file to host configuration. It's important to add this file here
			// in order to make this available hostBuilderContext for ConfigureAppConfiguration method
			// to read and add the additional configuration files from AdditionalJsonConfigs section
			// in appsettings file.
			var app_settings_file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
			if (File.Exists(app_settings_file) == true)
			{
				configBuilder.AddJsonFile(app_settings_file, optional: false, reloadOnChange: false);
			}
		});

		// Configure application configuration.
		builder.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
		{
			// Add application name to the environment.
			hostBuilderContext.HostingEnvironment.ApplicationName = applicationName;

			// Add environment specific appsettings file.
			// This could not be done in the ConfigureHostConfiguration method above because the HostingEnvironment is not available there. 
			var environment = hostBuilderContext.HostingEnvironment;
			configurationBuilder.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

			// Get any additional JSON config files provided via commandline args.
			var provided_addl_json_configs = (hostBuilderContext.Configuration.GetSection("AdditionalJsonConfigs").Get<IEnumerable<string>>() ?? Enumerable.Empty<string>()).ToList();
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
				configurationBuilder.AddJsonFile(json_config_file_path, optional: false, reloadOnChange: false);
			}

			// Configure configuration for external services.
			var service_injector_config_section = hostBuilderContext.Configuration.GetSection("ServiceInjector");
			Xssert.IsNotNull(service_injector_config_section, nameof(service_injector_config_section));
			DoGetServiceInjectorInstance(service_injector_config_section)?.ConfigureConfiguration(configurationBuilder);

			// Configure additional external assembly providers if any are configured in the config file.
			var file_providers = new List<IFileProvider>();
			var assembly_provider_section = hostBuilderContext.Configuration.GetSection("AssemblyProvider");
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

			// Add the base directory of the deployed application as a file provider.
			file_providers.Add(new PhysicalFileProvider(AppContext.BaseDirectory));

			var assembly_provider = new AssemblyProvider(new AppDomainLoadedAssembliesProvider(), new AssemblyLoadContextAssemblyLoader(new CompositeFileProvider(file_providers)));
			AssemblyLoadContext.Default.Resolving += (context, name) => assembly_provider.Get(name);
		});

		// Configure external services that need to be injected into the service collection.
		builder.ConfigureServices((hostBuilderContext, services) =>
		{
			// Add core services - ISequentialGuidGenerator, IJsonUtilities, and Serilog.
			services.AddCoreServices(hostBuilderContext.Configuration);

			// Inject any external service if an injector is configured
			var service_injector_config_section = hostBuilderContext.Configuration.GetSection("ServiceInjector");
			Xssert.IsNotNull(service_injector_config_section, nameof(service_injector_config_section));
			DoGetServiceInjectorInstance(service_injector_config_section)?.ConfigureServices(hostBuilderContext.Configuration, services);
		});

		return builder;
	}

	/// <summary>
	/// Creates a common application builder with injected core services like seri-logger, guid provider, and common services 
	/// requested to be injected through IServicesInjector implementation provided through config file.
	/// </summary>
	/// <param name="applicationName"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public static HostApplicationBuilder CreateHostApplicationBuilder(string applicationName, string[] args)
	{
		var host_application_builder_settings = new HostApplicationBuilderSettings
		{
			ApplicationName = applicationName,
			Args = args,
		};

		var builder = Host.CreateApplicationBuilder(host_application_builder_settings);
		builder.ConfigureApplicationBuilder(host_application_builder_settings);

		return builder;
	}

	/// <summary>
	/// This extension allows the other builders like WebApplicationBuilder 
	/// to configure the host exactly the same way as we construct the 
	/// HostApplicationBuilder.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="host_application_builder_settings"></param>
	public static void ConfigureApplicationBuilder(this IHostApplicationBuilder builder, HostApplicationBuilderSettings host_application_builder_settings)
	{
		builder.Configuration.AddCommandLine(host_application_builder_settings.Args ?? []);

		// Add appsettings file
		var app_settings_file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
		if (File.Exists(app_settings_file) == true)
		{
			builder.Configuration
				.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: false)
				.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
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

		// Configure additional external assembly providers if any are configured in the config file.
		var file_providers = new List<IFileProvider>();
		var assembly_provider_section = builder.Configuration.GetSection("AssemblyProvider");
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

		// Add the base directory of the deployed application as a file provider.
		file_providers.Add(new PhysicalFileProvider(AppContext.BaseDirectory));

		var assembly_provider = new AssemblyProvider(new AppDomainLoadedAssembliesProvider(), new AssemblyLoadContextAssemblyLoader(new CompositeFileProvider(file_providers)));
		AssemblyLoadContext.Default.Resolving += (context, name) => assembly_provider.Get(name);

		/* 
		 * NOTE: This is final point for injection of services in ServiceCollection and 
		 * no more service injection will work beyond this point because 
		 * hostApplicationBuilder.Services.BuildServiceProvider() will be 
		 * invoked after this point. 
		 */
	}

	#endregion - Public Methods -

	#region - Private Methods -

	private static IServicesInjector? DoGetServiceInjectorInstance(IConfigurationSection serviceInjectorConfigSection)
	{
		var service_injector_config = serviceInjectorConfigSection.Get<ServiceInjectorConfig>();
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

			var common_services_injector_instance = Activator.CreateInstance(common_services_injector_type) as IServicesInjector;
			Xssert.IsNotNull(common_services_injector_instance, nameof(common_services_injector_instance));

			return common_services_injector_instance;
		}

		return null;
	}

	/// <summary>
	/// Register any additional assembly providers along with the base directory of the deployed application.
	/// </summary>
	/// <param name="hostApplicationBuilder"></param>
	/// <param name="fileProvider"></param>
	private static void RegisterAssemblyProviders(IHostApplicationBuilder hostApplicationBuilder, IFileProvider? fileProvider = null)
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

		// Add the base directory of the deployed application as a file provider.
		file_providers.Add(new PhysicalFileProvider(AppContext.BaseDirectory));

		var assembly_provider = new AssemblyProvider(new AppDomainLoadedAssembliesProvider(), new AssemblyLoadContextAssemblyLoader(new CompositeFileProvider(file_providers)));
		AssemblyLoadContext.Default.Resolving += (context, name) => assembly_provider.Get(name);
	}

	/// <summary>
	/// Register any additional assembly providers. This must be called before the host is built. The responsibility of ensuring that
	/// the file provider(s) provided here contain the assemblies required for resolution is on the caller.
	/// </summary>
	/// <param name="hostApplicationBuilder"></param>
	/// <param name="fileProvider"></param>
	private static void RegisterAssemblyProviders(this IHostBuilder hostApplicationBuilder, IFileProvider? fileProvider = null)
	{
		var file_providers = new List<IFileProvider>();
		if (fileProvider != null)
		{
			file_providers.Add(fileProvider);
		}

		hostApplicationBuilder.ConfigureAppConfiguration((hostApplicationBuilder, config) =>
		{
			if (file_providers.Count > 0)
			{
				var assembly_provider = new AssemblyProvider(new AppDomainLoadedAssembliesProvider(), new AssemblyLoadContextAssemblyLoader(new CompositeFileProvider(file_providers)));
				AssemblyLoadContext.Default.Resolving += (context, name) => assembly_provider.Get(name);
			}
		});
	}

	#endregion - Private Methods -
}
