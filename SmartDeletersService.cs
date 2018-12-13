using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace YourProject.Repositories.Services
{
    public class SmartDeletersService
    {
        ApplicationDbContext _dbContext;

        public SmartDeletersService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        //This method will check if the entity is used by another entity in the database, usefull when you want to delete an entity
        //Please note that you need to pass a Type as second param
        
        public bool IsUsedBy<T1>(T1 entity, Type toCheckIn) where T1 : class
        {
            var proprityInfos = entity.GetType().GetProperties().Where(p => p.Name.Contains("Id"));
            PropertyInfo keyProperty = null;
            foreach (var item in proprityInfos)
            {
                var keyAttr = item
                            .GetCustomAttributes(typeof(KeyAttribute), false)
                            .Cast<KeyAttribute>().FirstOrDefault();
                if (keyAttr != null)
                {
                    keyProperty = item;
                    break;
                }
            }

            if (keyProperty != null)
            {
                var property = toCheckIn.GetProperties().Where(p => p.Name.Contains(keyProperty.Name)).FirstOrDefault();
                if (property != null)
                {
                    var t2TableName = _dbContext.Model.FindEntityType(toCheckIn).Relational().TableName;
                    var t1TableName = _dbContext.Model.FindEntityType(entity.GetType()).Relational().TableName;
                    int result = 0;
                    var sqlQuery = $"Select Count(*) from {t2TableName} where {keyProperty.Name} = {((int)keyProperty.GetValue(entity)).ToString()} AND IsDeleted = 0";
                    var connString = _dbContext.Database.GetDbConnection().ConnectionString;
                    SqlConnection conn = new SqlConnection("Server=130.117.45.25;Database=ImmoSalesTestDB;User Id=PeakTestUser;Password=%@Peak2018;");
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        conn.Open();
                        result = (int)cmd.ExecuteScalar();
                        conn.Close();

                        if (result != 0)
                            return true;
                    }

                }


            }

            return false;

        }

 //This method will check if the entity is used by other entities in the database, usefull when you want to delete an entity
 
        public bool IsUsedBy<T1>(T1 entity, List<Type> types) where T1 : class
        {
            var proprityInfos = entity.GetType().GetProperties().Where(p => p.Name.Contains("Id"));
            PropertyInfo keyProperty = null;
            foreach (var item in proprityInfos)
            {
                var keyAttr = item
                            .GetCustomAttributes(typeof(KeyAttribute), false)
                            .Cast<KeyAttribute>().FirstOrDefault();
                if (keyAttr != null)
                {
                    keyProperty = item;
                    break;
                }
            }

            if (keyProperty != null)
            {
                foreach (var toCheckIn in types)
                {
                    var property = toCheckIn.GetProperties().Where(p => p.Name.Contains(keyProperty.Name)).FirstOrDefault();
                    if (property != null)
                    {
                        var t2TableName = _dbContext.Model.FindEntityType(toCheckIn).Relational().TableName;
                        var t1TableName = _dbContext.Model.FindEntityType(entity.GetType()).Relational().TableName;
                        int result = 0;
                        var sqlQuery = $"Select Count(*) from {t2TableName} where {keyProperty.Name} = {((int)keyProperty.GetValue(entity)).ToString()} AND IsDeleted = 0";
                        var connString = _dbContext.Database.GetDbConnection().ConnectionString;
                        SqlConnection conn = new SqlConnection("Server=130.117.45.25;Database=ImmoSalesTestDB;User Id=PeakTestUser;Password=%@Peak2018;");
                        using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                        {
                            conn.Open();
                            result = (int)cmd.ExecuteScalar();
                            conn.Close();

                            if (result != 0)
                                return true;
                        }

                    }
                }



            }

            return false;

        }
    }
}
