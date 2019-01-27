using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CountingStrings.Service.Extensions
{
    public static class DbExtensions
    {
        public static bool IsConcurrencyException(this Exception ex)
        {
            return (ex is DbUpdateConcurrencyException) || ex.IsDuplicatedKeyException();
        }

        public static bool IsDuplicatedKeyException(this Exception ex)
        {
            return (ex.InnerException as SqlException)?.Number == 2627;
        }
    }
}
