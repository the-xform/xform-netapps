// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Data;

namespace XForm.NetApps.Interfaces;

public interface IDbContextProvider : IDisposable
{
	/// <summary>
	/// The name of the context provider. It is helpful if multiple dbcontext objects are
	/// used and the code needs to refer to specific one.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Returns the associated SqlConnection.
	/// </summary>
	IDbConnection? Connection { get; }

	/// <summary>
	/// Opens the database connection.
	/// </summary>
	void OpenConnection();

	/// <summary>
	/// Closes the database connection.
	/// </summary>
	void CloseConnection();

	/// <summary>
	/// Determines if any transaction is active.
	/// </summary>
	bool IsInTransaction { get; }

	/// <summary>
	/// Current transaction object.
	/// </summary>
	IDbTransaction? CurrentTransaction { get; }

	/// <summary>
	/// Begins a new transaction. Throws an error is there is no active transaction.
	/// Tries to open the underlying database connection if it closed.
	/// </summary>
	/// <returns></returns>
	IDbTransaction? BeginTransaction();

	/// <summary>
	/// Commits the current transaction.
	/// Does not close the underlying database connection.
	/// </summary>
	void CommitTransaction();

	/// <summary>
	/// Rolls back the current transaction. Throws an error is there is no active transaction.
	/// Does not close the underlying database connection.
	/// </summary>
	void RollbackTransaction();
}
