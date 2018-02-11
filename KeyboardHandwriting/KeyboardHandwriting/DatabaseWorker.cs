using KeyboardHandwriting.Models;
using System.Data.SqlClient;

namespace KeyboardHandwriting
{
    public static class DatabaseWorker
    {
        public const string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\Работа\Дипломы\1801296 Компьютерный подчерк\KeyboardHandwriting\KeyboardHandwriting\LocalDb.mdf;Integrated Security=True";

        public static User GetUser(string userName)
        {
            SqlConnection con = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(string.Format("select * from Users where UserName = '{0}'", userName), con);
            con.Open();
            SqlDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                return new User
                {
                    Id = int.Parse(reader["Id"].ToString()),
                    UserName = reader["UserName"].ToString()
                };
            }
            return null;
        }

        public static HandWriting GetUserHandwriting(int userId)
        {
            SqlConnection con = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(string.Format("select * from Handwriting where UserId = '{0}'", userId), con);
            con.Open();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                return new HandWriting
                {
                    Id = int.Parse(reader["Id"].ToString()),
                    UserId = int.Parse(reader["UserId"].ToString()),
                    Pauses = double.Parse(reader["Pauses"].ToString()),
                    Holding = double.Parse(reader["Holding"].ToString()),
                    ErrorsCount = double.Parse(reader["ErrorsCount"].ToString()),
                    Speed = double.Parse(reader["Speed"].ToString()),
                    Overlapping = double.Parse(reader["Overlapping"].ToString())
                };
            }
            return null;
        }


        public static void InsertUser(string userName)
        {
            SqlConnection con = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(string.Format(@"
                    insert into Users (UserName) 
                    values ('{0}');", userName), con);
            con.Open();
            command.ExecuteNonQuery();
        }


        public static void InserHandwriting(HandWriting handWriting)
        {
            SqlConnection con = new SqlConnection(ConnectionString);

            var ddd = string.Format("insert into Handwriting (UserId, Pauses, Holding, ErrorsCount, Speed, Overlapping) values ({0}, {1}, {2}, {3}, {4}, {5});",
                handWriting.UserId, handWriting.Pauses.ToString().Replace(",", "."),
                handWriting.Holding.ToString().Replace(",", "."), handWriting.ErrorsCount.ToString().Replace(",", "."),
                handWriting.Speed.ToString().Replace(",", "."), handWriting.Overlapping.ToString().Replace(",", "."));

            var da = string.Format("insert into Handwriting (UserId, Pauses, Holding, ErrorsCount, Speed, Overlapping) values ({0}, {1}, {2}, {3}, {4}, {5});",
                handWriting.UserId, handWriting.Pauses,
                handWriting.Holding, handWriting.ErrorsCount,
                handWriting.Speed, handWriting.Overlapping);

            SqlCommand command = new SqlCommand(string.Format("insert into Handwriting (UserId, Pauses, Holding, ErrorsCount, Speed, Overlapping) values ({0}, {1}, {2}, {3}, {4}, {5});",
                handWriting.UserId, handWriting.Pauses.ToString().Replace(",", "."), 
                handWriting.Holding.ToString().Replace(",", "."), handWriting.ErrorsCount.ToString().Replace(",", "."), 
                handWriting.Speed.ToString().Replace(",", "."), handWriting.Overlapping.ToString().Replace(",", ".")), con);
            con.Open();
            command.ExecuteNonQuery();
        }

    }
}
