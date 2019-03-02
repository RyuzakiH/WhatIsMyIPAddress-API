using System;
using System.Threading.Tasks;

namespace WhatIsMyIPAddress.API.Utilities
{
    public class Utilities
    {
        
        public static T TryMany<T>(Func<T> func, int tries)
        {
            T result = default(T);

            for (int i = 0; i < tries; i++)
            {
                try { result = func(); break; }
                catch (Exception) { continue; }
            }

            return result;
        }

        public async static Task<T> TryManyAsync<T>(Func<Task<T>> func, int tries)
        {
            T result = default(T);

            for (int i = 0; i < tries; i++)
            {
                try { result = await func(); break; }
                catch (Exception) { continue; }
            }

            return result;
        }

    }
}