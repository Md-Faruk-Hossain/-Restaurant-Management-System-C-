using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Restaurant_Management_System.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Restaurant_Management_System
{
    
    public partial class MainWindow : Window
    {
        public string filename = @"employee.json";
        public FileInfo TempImageFile { get; set; }
        public BitmapImage DefaultImage => new BitmapImage(new Uri(GetImagePath() + "default.png"));

       

        public MainWindow()
        {
            InitializeComponent();
            List<string> Designation = new List<string>()
            {
                  "Manager",
                  "Cashier",
                  "Waiter",
                  "Chef"
            };
            this.CmbDesignation.ItemsSource = Designation;
            CmbDesignation.Text = "Manager.";

            List<string> Gender = new List<string>()
            {
                  "Male",
                  "Female",

            };
            this.CmbGender.ItemsSource = Gender;
            CmbGender.Text = "Male.";

            var path = Path.GetDirectoryName(GetImagePath());
            if (!File.Exists(filename))
            {
                File.CreateText(filename).Close();
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            ImageShow.Source = DefaultImage;
            Show();         
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Employee employee = new Employee()
            {
                ImageTitle = (TempImageFile != null) ? $"{int.Parse(TextEmpId.Text) +TempImageFile.Extension}" : "default.png",
                EmployeeId = Convert.ToInt32(TextEmpId.Text),
                Name = TextName.Text,
                Designation = CmbDesignation.SelectedItem.ToString(),
                PhoneNo = TextPhoneNo.Text,
                Age = TextAge.Text,
                NationalId=TextNationalId.Text,
                Email = TextEmail.Text,
                Address = TextAddress.Text,
                Gender = CmbGender.SelectedItem.ToString(),
                City = TextCity.Text,
                Country = TextCountry.Text,
                Date = DateJoin.SelectedDate.Value.Date,

            };

            string filedata = File.ReadAllText(filename);
            if (IsValidJson(filedata) && IsExists("Employee") && !IsIdExists(employee.EmployeeId)) //check file contains valid json format and exists "Employee" Parent Node
            {
                var data = JObject.Parse(filedata);
                var empJson = data.GetValue("Employee").ToString();
                var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);
                empList.Add(employee);
                JArray empArray = JArray.FromObject(empList);
                data["Employee"] = empArray;
                var newJsonResult = JsonConvert.SerializeObject(data, Formatting.Indented);

                if (TempImageFile != null)
                {
                    TempImageFile.CopyTo(GetImagePath() + employee.ImageTitle);
                    TempImageFile = null;
                    ImageShow.Source = DefaultImage;
                }
                File.WriteAllText(filename, newJsonResult);     //write all employees to json file
            }

            if (!IsValidJson(filedata))
            {
                var emp = new { Employee = new Employee[] { employee } };  //create json format with parent[Employee]
                string newJsonResult = JsonConvert.SerializeObject(emp, Formatting.Indented);   //serialize json format
                if (TempImageFile != null)
                {
                    TempImageFile.CopyTo(GetImagePath() + employee.ImageTitle);
                    TempImageFile = null;
                    ImageShow.Source = DefaultImage;
                }
                File.WriteAllText(filename, newJsonResult);         //write json format to employee.json
            }
            Showdata();
           
        }
        private bool IsIdExists(int inputId)    //input id from input box
        {
            string filedata = File.ReadAllText(filename);
            var data = JObject.Parse((string)filedata);              //parse file data as JObject
            var empJson = data.GetValue("Employee").ToString();
            var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);

            var exists = empList.Find(x => x.EmployeeId == inputId);                 //return employee if id found, else return null

            if (exists != null)
            {
                MessageBox.Show($"EmployeeId - {exists.EmployeeId} exists\nTry with different EmployeeId", "Message", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
            else
            {
                return false;
            }

        }

        private bool IsValidJson(string data)   //check whether file contains json format or not
        {

            try
            {
                var temp = JObject.Parse(data);  //Try to parse json data if can't will throw exception
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsExists(string data)      //Check if exists parent node ('Employee') in json file
        {
            string filedata = File.ReadAllText(filename);
            var jsonObject = JObject.Parse(filedata);
            var empJson = jsonObject[data];     //If not exists return null

            return (empJson != null) ? true : false;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Update update = new Update();
            update.Show();
            this.Hide();
            update.Owner = this;

            Button b = sender as Button;
            Employee empbtn = b.CommandParameter as Employee;

            update.TextEmpId.IsEnabled = false;
            update.TextEmpId.Text = empbtn.EmployeeId.ToString();
            update.TextName.Text = empbtn.Name;
            update.CmbDesignation.Text = empbtn.Designation;
            update.TextPhoneNo.Text = empbtn.PhoneNo;
            update.TextAge.Text = empbtn.Age;
            update.TextNationalId.Text = empbtn.NationalId;
            update.TextEmail.Text = empbtn.Email;
            update.TextAddress.Text = empbtn.Address;
            update.CmbGender.Text = empbtn.Gender;
            update.TextCity.Text = empbtn.City;
            update.TextCountry.Text = empbtn.Country;
            update.DataContext = empbtn.Date;          
            update.ImageModify.Source = empbtn.ImageSrc;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var jsonD = File.ReadAllText(filename);
            var jsonObj = JObject.Parse(jsonD);
            var empJson = jsonObj.GetValue("Employee").ToString();
            var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);

            Button b = sender as Button;
            Employee empbtn = b.CommandParameter as Employee;
            int empId = empbtn.EmployeeId;

            MessageBoxResult result = MessageBox.Show($"Are you want to delete ID - {empId}", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes) //if press 'Yes' on delete confirmation
            {
                empList.Remove(empList.Find(x => x.EmployeeId == empId));   //Remove the employee from the list
                JArray empArray = JArray.FromObject(empList);       //Convert List<Employee> to JArray
                jsonObj["Employee"] = empArray;                    //Add JArray to 'Employee' JProperty
                var newJsonResult = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

                FileInfo thisFile = new FileInfo(GetImagePath() + empbtn.ImageTitle);
                if (thisFile.Name != "default.png") //Delete image (Not default image)
                {
                    thisFile.Delete();
                }

                File.WriteAllText(filename, newJsonResult);

                MessageBox.Show("Data Deleted Successfully !!", "Delete", MessageBoxButton.OK, MessageBoxImage.Question);
                Showdata();
                AllClear();
            }
            else
            {
                return;
            }
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            Showdata();
        }
        public void Showdata()
        {
            var json = File.ReadAllText(filename);

            if (!IsValidJson(json))
            {
                return;
            }

            var jsonObj = JObject.Parse(json);
            var empJson = jsonObj.GetValue("Employee").ToString();
            var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);   //Deserialize to List<Employee>
            empList = empList.OrderBy(x => x.EmployeeId).ToList();  //Sorting List<Employee> by Id (Ascending)

            foreach (var item in empList)
            {
                item.ImageSrc = ImageInstance(new Uri(GetImagePath() + item.ImageTitle));   //Create image instance for all Employee
            }
            ListEmployee.ItemsSource = empList;
            ListEmployee.Items.Refresh();

            GC.Collect();                   //Call garbage collector to release unused image instance resource
            GC.WaitForPendingFinalizers();
        }
        public ImageSource ImageInstance(Uri path)  //Create image instance rather than referencing image file
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = path;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.DecodePixelWidth = 300;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        public string GetImagePath()    //Get the Image Directory Path Where Image is stored
        {
            var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string assemblyDirectory = Path.GetDirectoryName(currentAssembly.Location);             // debug folder
            string ImagePath = Path.GetFullPath(Path.Combine(assemblyDirectory, @"..\..\Img\"));    // ..\..\ Navigate two levels up => Project folder

            return ImagePath;
        }

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Image Files(*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png;";
            fd.Title = "Select an Image";
            if (fd.ShowDialog().Value == true)
            {
                ImageShow.Source = new BitmapImage(new Uri(fd.FileName));
                TempImageFile = new FileInfo(fd.FileName);
            }
           
        }
        public void AllClear()
        {
            TextEmpId.Clear();
            CmbDesignation.SelectedIndex = -1;
            TextName.Clear();
            TextPhoneNo.Clear();
            TextAge.Clear();
            TextNationalId.Clear();
            TextEmail.Clear();
            TextAddress.Clear();
            CmbGender.SelectedIndex = -1;
            TextCity.Clear();
            TextCountry.Clear();
            TextEmpId.IsEnabled = true;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            View v = new View();
            this.Hide();
            v.Show();

            Button b = sender as Button;
            Employee empbtn = b.CommandParameter as Employee;
            v.TextView.Text = $"EmployeeId :\t{empbtn.EmployeeId }\nname :\t\t{empbtn.Name}\nDesignation :\t{empbtn.Designation} \nPhoneNo :\t{empbtn.PhoneNo}\nAge   :\t\t{empbtn.Age}\nNationalId :\t {empbtn.NationalId}\nEmail    :\t\t{empbtn.Email}\nGender :\t\t{empbtn.Gender}\nCity   :\t\t{empbtn.City}\nCountry   :\t {empbtn.Country}\nJoinDate  :\t {empbtn.Date}";
            v.imageBox.Source = ImageInstance(new Uri(GetImagePath() + empbtn.ImageTitle));
        }
    }
    
}
