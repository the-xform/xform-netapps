// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XForm.NetApps.Interfaces;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Attibutes.Web;

public class DbContextAttribute : ActionFilterAttribute
{
	private readonly bool _executeInTransaction;
	private readonly string _dbConnectionStringName;

	/// <summary>
	/// Instantiates the DbContext attribute with the specified connection string name and transaction option.
	/// </summary>
	/// <param name="executeInTransaction"></param>
	/// <param name="dbConnectionStringName">The name of the connection string from app settings. Default is "XformConnectionString".</param>
	public DbContextAttribute(bool executeInTransaction = true, string dbConnectionStringName = "XformConnectionString")
	{
		_executeInTransaction = executeInTransaction;
		_dbConnectionStringName = dbConnectionStringName;
	}

	public override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
	{
		var logger = actionExecutingContext.HttpContext.RequestServices.GetRequiredService<ILogger<DbContextAttribute>>();
		Xssert.IsNotNull(logger);

		try
		{
			logger.LogDebug($"{nameof(OnActionExecuting)}: Initializing db connection using named connection string '{_dbConnectionStringName}'");

			var db_context = actionExecutingContext.HttpContext.RequestServices.GetKeyedService<IDbContextProvider>(_dbConnectionStringName);
			Xssert.IsNotNull(db_context?.Connection);

			if (db_context.Connection.State == System.Data.ConnectionState.Closed)
			{
				logger.LogDebug($"{nameof(OnActionExecuting)}: Opening db connection ...'");
				db_context.Connection.Open();
				logger.LogDebug($"{nameof(OnActionExecuting)}: Opened db connection.'");
			}

			if (_executeInTransaction == true)
			{
				logger.LogDebug($"{nameof(OnActionExecuting)}: Creating new db transaction ...'");
				db_context.BeginTransaction();
				logger.LogDebug($"{nameof(OnActionExecuting)}: Created new db transaction.'");
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"{nameof(OnActionExecuting)}: Exception occurred while initiating transaction on db context.");
			throw;
		}
		finally
		{
			base.OnActionExecuting(actionExecutingContext);
		}
	}

	public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
	{
		var logger = actionExecutedContext.HttpContext.RequestServices.GetRequiredService<ILogger<DbContextAttribute>>();
		Xssert.IsNotNull(logger);

		var db_context = actionExecutedContext.HttpContext.RequestServices.GetKeyedService<IDbContextProvider>(_dbConnectionStringName);
		Xssert.IsNotNull(db_context?.Connection);

		try
		{
			logger.LogDebug($"{nameof(OnActionExecuted)}: Initializing db connection using named connection string '{_dbConnectionStringName}'");

			if (actionExecutedContext.Exception == null
				&& actionExecutedContext.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK)
			{
				// Commit transaction if any
				if (db_context.IsInTransaction == true)
				{
					logger.LogDebug($"{nameof(OnActionExecuted)}: Committing db transaction ...'");
					db_context.CommitTransaction();
					logger.LogDebug($"{nameof(OnActionExecuted)}: Commited db transaction.'");
				}
			}
			else
			{
				// Rollback transaction if any
				if (db_context.IsInTransaction == true)
				{
					logger.LogWarning($"{nameof(OnActionExecuted)}: Rolling back transaction due to an error reported in response ...");
					db_context.RollbackTransaction();
					logger.LogWarning($"{nameof(OnActionExecuted)}: Rolled back transaction due to an error reported in response.");
				}
			}

			// Close the db connections.
			if (db_context.Connection.State == System.Data.ConnectionState.Open)
			{
				logger.LogDebug($"{nameof(OnActionExecuted)}: Closing db connection ...'");
				db_context.Connection.Close();
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"{nameof(OnActionExecuted)}: Exception occurred while finishing transaction and/or closing db context.");
			
			if (db_context.IsInTransaction == true)
			{
				logger.LogWarning($"{nameof(OnActionExecuted)}: Rolling back transaction due to an error ...");
				db_context.RollbackTransaction();
				logger.LogWarning($"{nameof(OnActionExecuted)}: Rolled back transaction due to an error. See exception log above.");
			}

			throw;
		}
		finally
		{
			base.OnActionExecuted(actionExecutedContext);
		}
	}
}
