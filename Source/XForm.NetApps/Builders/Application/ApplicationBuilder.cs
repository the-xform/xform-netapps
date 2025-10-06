// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

namespace XForm.Common.Apps.Builders.Application;

public static class ApplicationBuilder
{
    public static void ConfigureThreadsForApplication(int minWorkerThreads, int minPortThreads)
    {
        ThreadPool.GetMinThreads(out var min_threads, out var min_port_threads);
        ThreadPool.SetMinThreads(min_threads < minWorkerThreads ? minWorkerThreads : min_threads, 
                                 min_port_threads < minPortThreads ? minPortThreads : min_port_threads);
    }
}