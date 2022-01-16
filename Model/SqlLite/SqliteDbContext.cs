using SQLite.CodeFirst;
using System;
using System.Data.Entity;
using System.Data.SQLite;

namespace Model.SqlLite
{
    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext(string connectionString) : base(new SQLiteConnection() { ConnectionString = connectionString }, false)
        {

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            try
            {
                modelBuilder.Configurations.Add(new EventInboxConfiguration());
                var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<SqliteDbContext>(modelBuilder);
                Database.SetInitializer(sqliteConnectionInitializer);
            }
            catch (Exception ex)
            {
                Exception exception = new Exception("Не удалось корректно создать файл БД.\nПодробности см. в журнале ошибок.", ex);
                throw exception;
            }
        }
        public new void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected override void Dispose(bool disposing)
        {
            var connection = this.Database.Connection;
            base.Dispose(disposing);
            connection.Dispose();
        }
        //public DbSet<IEventInbox> EventInboxes { get; set; }
    }
}
