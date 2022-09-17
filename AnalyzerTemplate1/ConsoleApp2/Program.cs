namespace Test
{    
    public class Student
    {
        private readonly string _name;
        public Student(string name)
        {
            _name = name;
        }

        public string GetStudent()
        {
            return _name;
        }
    }

    public class TestClass
    {
        static void Main() { }
        public Student TryCreationMe()
        {
            return new Student(string.Empty);
        }
    }
}
