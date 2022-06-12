using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;


//Niestety niedokończone, bo zabrakło czasu

namespace Zad5
{
    public class SqlServerDbService : IDbService

    {
        private readonly IConfiguration configuration;
        public SqlServerDbService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<int> AddWarehouse(Warehouse warehouse)
        {
            using SqlConnection connection = new SqlConnection(configuration.GetConnectionString("DefaultDbConnection"));
            using (SqlCommand com = new())
            {
                com.Connection = connection;
                await connection.OpenAsync();
                var transaction = await connection.BeginTransactionAsync();
                com.Transaction = transaction as SqlTransaction;
                try
                {
                    //TASK 1
                    com.CommandText = $"SELECT * FROM Product WHERE IdProduct = @productId";
                    com.Parameters.AddWithValue("@productId", warehouse.IdProduct);
                    var rowsAffectedSelectProduct =  com.ExecuteNonQuery();
                    if (rowsAffectedSelectProduct > 0)
                    {
                        throw new Exception("Istnieje produkt o podanym id");
                    }

                    com.Parameters.Clear();
                    com.CommandText = $"SELECT * FROM Warehouse WHERE IdWareHouse = @warehouseId";
                    com.Parameters.AddWithValue("@warehouseId", warehouse.IdWarehouse);
                    var rowsAffectedSelectWarehouse = com.ExecuteNonQuery();
                    if (rowsAffectedSelectWarehouse > 0)
                    {
                        throw new Exception("Istnieje hurtownia o podanym id");
                    }
                    if(warehouse.Amount < 1)
                    {
                        throw new Exception("Wartość kwoty musi być większa niż 1");
                    }

                    //TASK 2
                    com.Parameters.Clear();
                    com.CommandText = $"SELECT * FROM [s21173].[dbo].[Order] WHERE IdProduct = @productId AND Amount = @amount";
                    com.Parameters.AddWithValue("@productId", warehouse.IdProduct);
                    com.Parameters.AddWithValue("@amount", warehouse.Amount);
                    var rowsAffectedSelectOrder = com.ExecuteNonQuery();
                    if(rowsAffectedSelectOrder == 0)
                    {
                        throw new Exception("Brak zlecenia zakupu produktu- nie można dodać produktu");
                    }
                    com.Parameters.Clear();
                    com.CommandText = $"SELECT * FROM [s21173].[dbo].[Order] Join Product_Warehouse ON [s21173].[dbo].[Order].IdProduct = Product_Warehouse.IdProduct WHERE [s21173].[dbo].[Order].IdProduct = @productId AND [s21173].[dbo].[Order].CreatedAt <  @createdAt";
                    com.Parameters.AddWithValue("@productId", warehouse.IdProduct);
                    com.Parameters.AddWithValue("@createdAt", warehouse.CreatedAt);
                    var rowsAffectedSelectOrdersDate = com.ExecuteNonQuery();
                    if (rowsAffectedSelectOrdersDate == 0)
                    {
                        throw new Exception("Data utworzenia zamówienia nie jest mniejsza niż dzisiejsza data");
                    }

                    //TASK 3
                    com.Parameters.Clear();
                    com.CommandText = $"SELECT * FROM Product_Warehouse WHERE IdOrder = @idOrder";
                    com.Parameters.AddWithValue("@idOrder", warehouse.IdOrder);
                    var rowsAffectedSelectOrdersId = com.ExecuteNonQuery();
                    if(rowsAffectedSelectOrdersId > 0)
                    {
                        throw new Exception("Zlecenie zostało zrealizowane juz wcześniej");
                    }

                    //TASK 4
                    com.Parameters.Clear();
                    com.CommandText = $"UPDATE [s21173].[dbo].[Order] SET FulfilledAt = @date";
                    com.Parameters.AddWithValue("@date", warehouse.CreatedAt);
                    com.ExecuteNonQuery();

                    //TASK 5
                    com.Parameters.Clear();
                    com.CommandText = $"INSERT INTO Product(IdProduct) values(Name, Description, Price) SELECT SCOPE_IDENTITY()";
                    com.Parameters.AddWithValue("@idProduct", warehouse.IdProduct);
                    com.ExecuteNonQuery();
                    var a = com.ExecuteReader().Read();


                    com.Parameters.Clear();
                    com.CommandText = $"INSERT INTO Warehouse(IdWareHouse) values(@idWarehouse)";
                    com.Parameters.AddWithValue("@idWarehouse", warehouse.IdWarehouse);
                    com.ExecuteNonQuery();

                    com.Parameters.Clear();
                    com.CommandText = $"INSERT INTO Product_Warehouse(Amount) values(@amount)";
                    com.Parameters.AddWithValue("@amount", warehouse.Amount);
                    com.ExecuteNonQuery();

                    com.Parameters.Clear();
                    com.CommandText = $"INSERT INTO Product_Warehouse(CreatedAt) values(@created)";
                    com.Parameters.AddWithValue("@created", DateTime.Now);
                    com.ExecuteNonQuery();

                    com.Parameters.Clear();
                    com.CommandText = $"SELECT Price FROM Product WHERE IdProduct = @productId";
                    com.Parameters.AddWithValue("@productId", warehouse.IdProduct);
                    var price = 0;
                    SqlDataReader dr = com.ExecuteReader();
                    while (dr.Read())
                    {
                        price = int.Parse(dr["Price"].ToString());
                    }

                    com.Parameters.Clear();
                    com.CommandText = $"INSERT INTO Product_Warehouse(Price) values(@price)";
                    com.Parameters.AddWithValue("@price", warehouse.Amount * price);

                    var primaryKey = int.Parse((await com.ExecuteScalarAsync()).ToString());
                    await transaction.CommitAsync();
                    return primaryKey;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }
    }

}