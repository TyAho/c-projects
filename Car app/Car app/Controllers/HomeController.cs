using Car_app.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString = @"Data Source=DESKTOP-FIQH8LT;Initial Catalog=CarQuotes;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public int agePrice { get; private set; }
        public int yearPrice { get; private set; }
        public int makePrice { get; private set; }
        public int modelPrice { get; private set; }
        public double duiPercent { get; private set; }
        public double coveragePercent { get; private set; }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Quote(string firstName, string lastName, string emailAddress,
                                    string dateOfBirth, string carYear, string carMake,
                                    string carModel, string tickets, string dUI, string coverage)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(emailAddress)
                || string.IsNullOrEmpty(dateOfBirth) || string.IsNullOrEmpty(carYear) ||
                string.IsNullOrEmpty(carMake) || string.IsNullOrEmpty(carModel) || string.IsNullOrEmpty(tickets)
                || string.IsNullOrEmpty(dUI) || string.IsNullOrEmpty(coverage))
            {
                return View("~/Views/Shared/Error.cshtml");
            }
            else
            {
                int initial = 50;
                int age = DateTime.Now.Year - Convert.ToDateTime(dateOfBirth).Year;
                if (age < 18)
                {
                    agePrice = 100;
                }
                if (age < 25 && age > 18)
                {
                    agePrice = 25;
                }
                if (age > 100)
                {
                    agePrice = 25;
                }
                int year = Convert.ToInt32(carYear);
                if (year < 2000)
                {
                    yearPrice = 25;
                }
                if (year > 2015)
                {
                    yearPrice = 25;
                }
                if (carMake == "Porsche" || carMake == "porsche")
                {
                    makePrice = 25;
                }
                if (carMake == "Porsche" || carMake == "porsche" && carModel == "911 Carrera" || carModel == "911 carrera")
                {
                    modelPrice = 25;
                }
                int ticketPrice = Convert.ToInt32(tickets) * 10;

                int firstTotal = initial + agePrice + yearPrice + makePrice + modelPrice + ticketPrice;

                if (dUI == "yes")
                {
                    duiPercent = firstTotal * .25;
                }

                double secondTotal = duiPercent + firstTotal;

                if (coverage == "full")
                {
                    coveragePercent = secondTotal * .5;
                }

                double finalTotal = secondTotal + coveragePercent;
                double moneyTotal = Math.Truncate(finalTotal * 100) / 100;
                string Total = moneyTotal.ToString();


                string queryString = @"INSERT INTO CarData (FirstName, LastName, EmailAddress, DateOfBirth, CarYear, CarMake, CarModel, Tickets, DUI, Coverage, Quote) VALUES
                                        (@FirstName, @LastName, @EmailAddress, @DateOfBirth, @CarYear, @CarMake, @CarModel, @Tickets, @DUI, @Coverage, @Total)";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.Add("@FirstName", SqlDbType.VarChar);
                    command.Parameters.Add("@LastName", SqlDbType.VarChar);
                    command.Parameters.Add("@EmailAddress", SqlDbType.VarChar);
                    command.Parameters.Add("@DateOfBirth", SqlDbType.Date);
                    command.Parameters.Add("@CarYear", SqlDbType.VarChar);
                    command.Parameters.Add("@CarMake", SqlDbType.VarChar);
                    command.Parameters.Add("@CarModel", SqlDbType.VarChar);
                    command.Parameters.Add("@Tickets", SqlDbType.Int);
                    command.Parameters.Add("@DUI", SqlDbType.VarChar);
                    command.Parameters.Add("@Coverage", SqlDbType.VarChar);
                    command.Parameters.Add("@Total", SqlDbType.VarChar);

                    command.Parameters["@FirstName"].Value = firstName;
                    command.Parameters["@LastName"].Value = lastName;
                    command.Parameters["@EmailAddress"].Value = emailAddress;
                    command.Parameters["@DateOfBirth"].Value = dateOfBirth;
                    command.Parameters["@CarYear"].Value = carYear;
                    command.Parameters["@CarMake"].Value = carMake;
                    command.Parameters["@CarModel"].Value = carModel;
                    command.Parameters["@Tickets"].Value = tickets;
                    command.Parameters["@DUI"].Value = dUI;
                    command.Parameters["@Coverage"].Value = coverage;
                    command.Parameters["@Total"].Value = Total;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                return View("Success");
            }

        }

        public ActionResult Admin()
        {
            string queryString = @"SELECT Id, FirstName, LastName, EmailAddress, DateOfBirth, CarYear, CarMake, CarModel, Tickets, DUI, Coverage, Quote from CarData";
            List<Quote> quotes = new List<Quote>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Quote quote = new Quote();
                    quote.Id = Convert.ToInt32(reader["Id"]);
                    quote.FirstName = reader["FirstName"].ToString();
                    quote.LastName = reader["LastName"].ToString();
                    quote.EmailAddress = reader["EmailAddress"].ToString();
                    quote.DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]);
                    quote.CarYear = reader["CarYear"].ToString();
                    quote.CarMake = reader["CarMake"].ToString();
                    quote.CarModel = reader["CarModel"].ToString();
                    quote.Tickets = Convert.ToInt32(reader["Tickets"]);
                    quote.DUI = reader["DUI"].ToString();
                    quote.Coverage = reader["Coverage"].ToString();
                    quote.Total = reader["Quote"].ToString();
                    quotes.Add(quote);
                }
            }

            return View(quotes);
        }
    }
}