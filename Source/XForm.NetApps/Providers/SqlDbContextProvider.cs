using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;
using XForm.NetApps.Interfaces;

namespace XForm.NetApps.Providers;

public class SqlDbContextProvider : IDbContextProvider
{
	private readonly string _providerContextName;
	private readonly string _connectionString;
	private IDbConnection? _dbConnection;
	private IDbTransaction? _transaction;

	// Track whether Dispose has been called
	private bool _disposed;

	/// <summary>
	/// Instantiate a new SqlDbContextProvider with the given name and connectionstring.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="connectionString"></param>
	public SqlDbContextProvider(string name, string connectionString)
	{
		_providerContextName = name;
		_connectionString = connectionString;
		_dbConnection = new SqlConnection(_connectionString);
	}

	/// <summary>
	/// Returns the associated SqlConnection.
	/// </summary>
	public string Name => _providerContextName;

	/// <summary>
	/// Returns the associated SqlConnection.
	/// </summary>
	public IDbConnection? Connection => _dbConnection;

	/// <summary>
	/// Opens the database connection.
	/// </summary>
	public void OpenConnection()
	{
		if (_dbConnection?.State == ConnectionState.Closed)
		{
			_dbConnection.Open();
		}
	}

	/// <summary>
	/// Closes the database connection.
	/// </summary>
	public void CloseConnection()
	{
		if (_dbConnection?.State == ConnectionState.Open)
		{
			_dbConnection.Close();
		}
	}

	/// <summary>
	/// Determines if any transaction is active.
	/// </summary>
	public bool IsInTransaction => (_transaction != null);

	/// <summary>
	/// Current transaction object.
	/// </summary>
	public IDbTransaction? CurrentTransaction => _transaction;

	/// <summary>
	/// Begins a new transaction. Throws an error is there is no active transaction.
	/// Tries to open the underlying database connection if it closed.
	/// </summary>
	/// <returns></returns>
	public IDbTransaction BeginTransaction()
	{
		if (_transaction == null)
		{
			if (_dbConnection == null)
			{
				throw new InvalidOperationException("The database connection has not been initialized.");
			}

			if (_dbConnection.State == ConnectionState.Closed)
			{
				_dbConnection.Open();
			}

			_transaction = _dbConnection.BeginTransaction();
			return _transaction;
		}
		else
		{
			throw new Exception($"A transaction is already in progress.");
		}
	}

	/// <summary>
	/// Commits the current transaction.
	/// Does not close the underlying database connection.
	/// </summary>
	public void CommitTransaction()
	{
		if (_transaction == null)
		{
			throw new InvalidOperationException($"There is no active transaction to commit.");
		}

		try
		{
			if (_transaction.Connection?.State != ConnectionState.Open)
			{
				throw new Exception($"The underlying connection is either null or it is already closed.");
			}

			_transaction.Commit();
		}
		catch
		{
			throw;
		}
		finally
		{
			_transaction = null;
		}
	}

	/// <summary>
	/// Rolls back the current transaction. Throws an error is there is no active transaction.
	/// Does not close the underlying database connection.
	/// </summary>
	public void RollbackTransaction()
	{
		if (_transaction == null)
		{
			throw new InvalidOperationException($"There is no active transaction to commit.");
		}

		try
		{
			if (_transaction.Connection?.State != ConnectionState.Open)
			{
				throw new Exception("The underlying connection is either null or it is already closed.");
			}

			_transaction.Rollback();
		}
		catch
		{
			throw;
		}
		finally
		{
			_transaction = null;
		}
	}


	/// <summary>
	/// Disposes the transaction object and also closes the underlying connection.
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>
	/// Protected virtual dispose pattern for any derived classes to use if they need to.
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Release transaction object
			if (IsInTransaction == true)
			{
				_transaction?.Rollback();
				_transaction?.Dispose();
			}
			_transaction = null;

			// Release database connection
			if (_dbConnection?.State != ConnectionState.Closed)
			{
				_dbConnection?.Close();
			}
			_dbConnection?.Dispose();
		}

		_disposed = true;
	}
}
