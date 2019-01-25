using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CountingStrings.Service.Extensions
{
    public static class DbExtensions
    {
        public static bool IsConcurrencyProblem(this Exception ex)
        {
            return (ex is DbUpdateConcurrencyException) || (ex.InnerException as SqlException)?.Number == 2627;
        }
    }
}
