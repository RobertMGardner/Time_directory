using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace time_manager
{

    public class Employee
    {
        public int ID { get; set; }
        public int LoginCode { get; set; }
        public string EmployeeName { get; set; }
        public int StartDate { get; set; }
        public string ClockState { get; set; }

        public virtual ICollection<WorkDay> WorkDays { get; set; }
    }

    public class WorkDay
    {
        public int ID { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public int StartBreak { get; set; }
        public int EndBreak { get; set; }
        public int StartLunch { get; set; }
        public int EndLunch { get; set; }
        public int ClockState { get; set; }

        public Employee ActiveEmployee { get; set; }

    }

    public class EmployeeContext : DbContext
    {
        public EmployeeContext() : base()
        {
            Database.SetInitializer<EmployeeContext>(new DropCreateDatabaseIfModelChanges<EmployeeContext>());
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkDay> EmployeeWorkDays { get; set; }

    }


    class Program
    {

        public static RobUtil ru = new RobUtil();
        public static string employeeLogin;
        public static List<Employee> employeeList;
        public static Employee activeEmployee;
        public static int activeEmployeeID;
        public static string employeeState;

        public static string timeClock;




        static void Main(string[] args)
        {

            using (var ctx = new EmployeeContext())
            {
                var employee = new Employee() { LoginCode = 111111 };
                employee.ClockState = "OUT";
                ctx.Employees.Add(employee);
                ctx.SaveChanges();
            }


            PopulateEmployeeList();
            Welcome();
        }

        static void PopulateEmployeeList()
        {
            using (var ctx = new EmployeeContext())
            {
                employeeList = ctx.Employees
                    .SqlQuery("Select * from Employees")
                    .ToList<Employee>();
            }

        }

        static void Welcome()
        {
            Console.WriteLine("Welcome to Time Manager!");
            GetLogin();

        }

        static void GetLogin()
        {

            Console.WriteLine("Please enter your 6-digit employee code");
            employeeLogin = Console.ReadLine();
            bool validID = CheckID(employeeLogin);

            while (!validID)
            {
                Console.Clear();
                ru.WriteLineColor("[Login Failed: Employee ID does not exist]", ConsoleColor.Red);
                Console.WriteLine("Please enter your 6-digit employee code");
                employeeLogin = Console.ReadLine();
                validID = CheckID(employeeLogin);
            }
            Console.Clear();
            ru.WriteLineColor("[Login Succesfull]", ConsoleColor.Yellow);
            ru.WriteLineColor("Press any key to continue...", ConsoleColor.Gray);
            Console.ReadKey();

            MainMenu();
        }

        public static bool CheckID(string employeeLogin)
        {
            using (var ctx = new EmployeeContext())
            {
                foreach (Employee e in ctx.Employees)
                {
                    if (employeeLogin == e.LoginCode.ToString())
                    {
                        activeEmployeeID = e.ID;
                        return true;
                    }
                }

                return false;
            }

        }

        static void MainMenu()
        {
            Console.Clear();
            using (var ctx = new EmployeeContext())
            {
                activeEmployee = ctx.Employees.Find(activeEmployeeID);
                employeeState = activeEmployee.ClockState;
            }
            ru.WriteLineColor(employeeState, ConsoleColor.Blue);
            Console.WriteLine("Please choose from the options below.");
            Console.WriteLine("1: Clock In");
            Console.WriteLine("2: Clock Out");
            //Console.WriteLine("3. View Report");
            Console.WriteLine("ESC: Logout");

            var inputKey = Console.ReadKey();

            switch (inputKey.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    ClockIn(employeeState);
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    ClockOut(employeeState);
                    break;
            }

        }


        static void ClockIn(string clockState)
        {
            if (clockState == "IN")
            {
                Console.Clear();
                Console.WriteLine("You are already clocked in!");
                Console.ReadKey();
                MainMenu();
            }
            else
            {
                clockState = "IN";
                Console.Clear();

                using (var ctx = new EmployeeContext())
                {
                    var original = ctx.Employees.Find(activeEmployee.ID);
                    original.ClockState = "IN";
                    ctx.SaveChanges();
                }

                CreateWorkDay();

                Console.WriteLine("Successfully Clocked in.");
                Console.ReadKey();
                MainMenu();
            }
        }

        static void ClockOut(string clockState)
        {
            if (clockState == "OUT")
            {
                Console.Clear();
                Console.WriteLine("You are already clocked out!");
                Console.ReadKey();
                MainMenu();
            }
            else
            {
                clockState = "OUT";
                Console.Clear();

                using (var ctx = new EmployeeContext())
                {
                    var original = ctx.Employees.Find(activeEmployee.ID);
                    original.ClockState = "OUT";
                    ctx.SaveChanges();
                }

                Console.WriteLine("Successfully Clocked out.");
                Console.ReadKey();
                MainMenu();
            }

        }

        static void CreateWorkDay()
        {
            using (var ctx = new EmployeeContext())
            {
                //get current 0000 time
                DateTime localTime = DateTime.Now;
                int hours = localTime.Hour;
                Console.WriteLine(hours);  //debug
                int minutes = localTime.Minute;
                Console.WriteLine(minutes); // debug
                timeClock = $"{hours}{minutes}";

                var workday = new WorkDay() { ActiveEmployee = ctx.Employees.Find(activeEmployee.ID) };
                workday.StartTime = Convert.ToInt32(timeClock);
                ctx.EmployeeWorkDays.Add(workday);
                ctx.SaveChanges();
            }
        }

    }
}
