// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xform.Utilities.Scheduling;
using XForm.NetApps.Builders.WinService.Interfaces;

namespace XForm.NetApps.Builders.WinService;

public class WorkerHost : BackgroundService
{
	private readonly IWorker _worker;
	private readonly ILogger<WorkerHost> _logger;
	private readonly ISchedule _schedule;

	public WorkerHost(
		ILogger<WorkerHost> logger,
		IWorker worker,
		ISchedule schedule)
	{
		_worker = worker;
		_logger = logger;
		_schedule = schedule;
	}

	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		await Task.Yield();

		try
		{
			await DoWorkAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
		{
			// Do nothing here because task was canceled externally.
			_logger.LogInformation(ex, $"Exiting the work loop of worker host because the task was canceled.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, ex.Message);
			throw;
		}
	}

	private async Task DoWorkAsync(CancellationToken cancellationToken)
	{
		var executed_once = false;

		cancellationToken.ThrowIfCancellationRequested();

		// TODO: RA: Make this configurable too in worker settings.
		// Make sure that the worker runs at least once when it starts before starting to follow the schedule.
		await _worker.ExecuteAsync(cancellationToken).ConfigureAwait(false);

		if (_schedule.Type == ScheduleType.OneShot)
		{
			// If the schedule is OneShot, we exit after the first execution.
			return;
		}

		while (true)
		{

			(bool isActive, TimeSpan offsetToNext) = _schedule.CalculateOffsetToNextInterval(DateTime.Now);

			// The schedule is done when isActive is false ad the offset is negative.
			if (isActive == false && offsetToNext.Ticks < 0
				|| isActive == false && _schedule.Type == ScheduleType.OneShot)
			{
				break;
			}

			// There's a chance that offsetToNext is 0, always in the case of OneShot workers. Task.Delay(0) returns a Task.CompletedTask, which does not put us in the async pool.
			// We skip the execution if either the job was a OneShot job or the time interval was set to 0. Interval of 0 is not supported for performance reasons.
			var task = Task.Delay(offsetToNext, cancellationToken).ConfigureAwait(false);
			if (task.GetAwaiter().IsCompleted
				&& executed_once == true)
			{
				continue;
			}
			else
			{
				await task;
			}

			// Delegate to the actual worker.
			await _worker.ExecuteAsync(cancellationToken).ConfigureAwait(false);

			executed_once = true;
		}
	}
}
