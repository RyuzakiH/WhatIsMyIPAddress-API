using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zero.WhatIsMyIPAddress
{
    public class ObservableTask
    {
        public event EventHandler TaskCompleted;

        public void Run(Task task)
        {
            task.Start();

            task.Wait();

            TaskCompleted?.Invoke(this, null);
        }
    }

    public class ParallelTasks
    {
        public static void ExecuteParallelTasks(IEnumerable<Task> tasks, int threads_count)
        {
            var otasks = Enumerable.Range(0, threads_count).Select(t => new ObservableTask()).ToList();

            var tasks_count = tasks.Count();
            var completed_tasks = 0;

            for (int i = threads_count, j = 0; j < threads_count; j++)
            {
                otasks[j].TaskCompleted += (obj, ev) => { if (i < tasks_count) otasks[j].Run(tasks.ElementAt(i++)); completed_tasks++; };
                otasks[j].Run(tasks.ElementAt(j));
            }

            while (completed_tasks != tasks_count)
                Thread.Sleep(100);

            //Task.WhenAll(tasks).Wait();
        }
    }
}
