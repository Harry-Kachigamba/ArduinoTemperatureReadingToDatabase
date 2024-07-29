using System;
using System.IO.Ports;
using System.Threading.Tasks;
using MySqlConnector;

namespace TemperatureReadingToDatabase
{
	public class Program
	{
		private static SerialPort _serialPort;

		static void Main(string[] args)
		{
			try
			{
				_serialPort = new SerialPort("COM7", 9600);
				_serialPort.DataReceived += DataReceivedHandler;
				_serialPort.Open();

				Console.WriteLine("Listening for data...");
				Console.ReadLine(); //Keep the application running
			}
			catch (UnauthorizedAccessException ex)
			{
				Console.WriteLine($"Access to port is denied: {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occured: {ex.Message}");
			}
		}

		private static async void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				string data = _serialPort.ReadLine();
				Console.WriteLine($"Data Received: {data}");

				if (data.StartsWith("Temperature: "))
				{
					string temperatureString = data.Replace("Temperature: ", "").Trim();
					await SaveDataToDatabaseAsync(temperatureString);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred while reading data: {ex.Message}");
			}
		}

		private static async Task SaveDataToDatabaseAsync(string temperature)
		{
			try
			{
				string connectionString = "Server=localhost;port=3307;Database=RoomTemperature;UserID=root;Password=12345;Pooling=true;";

				using (var connection = new MySqlConnection(connectionString))
				{
					await connection.OpenAsync();

					string query = "INSERT INTO temperature_log(temperature, timestamp) VALUES (@temperature, NOW())";

					using (var command = new MySqlCommand(query, connection))
					{
						command.Parameters.AddWithValue("@temperature", temperature);

						int result = await command.ExecuteNonQueryAsync();

						if (result > 0)
						{
							Console.WriteLine("Data saved to database successfully.");
						}

						else
						{
							Console.WriteLine("Failed to save data to database.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occured while saving data to the database: {ex.Message}");
			}
		}
	}
}
