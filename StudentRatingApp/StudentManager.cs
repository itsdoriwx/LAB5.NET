using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRatingApp
{
    public class StudentManager
    {
        private List<Student> _students;
        private IFileLoader _fileLoader;
        private IFileSaver _fileSaver;

        public StudentManager(IFileLoader fileLoader, IFileSaver fileSaver)
        {
            if (fileLoader == null)
            {
                throw new ArgumentNullException(nameof(fileLoader), "Завантажувач файлів не може бути null.");
            }

            if (fileSaver == null)
            {
                throw new ArgumentNullException(nameof(fileSaver), "Сервіс збереження не може бути null.");
            }

            _fileLoader = fileLoader;
            _fileSaver = fileSaver;
            _students = new List<Student>();
        }

        public void AddStudent(Student student)
        {
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student), "Студент не може бути null.");
            }

            foreach (var existing in _students) // перевірка на дублікат студента
            {
                bool sameName = string.Equals(existing.FullName, student.FullName, StringComparison.OrdinalIgnoreCase);
                bool sameCourse = existing.Course == student.Course;
                bool sameGroup = string.Equals(existing.Group, student.Group, StringComparison.OrdinalIgnoreCase);

                if (sameName && sameCourse && sameGroup)
                {
                    throw new InvalidOperationException($"Студент «{student.FullName}» (курс: {student.Course}, група: {student.Group}) вже є у списку.");
                }
            }

            _students.Add(student);
        }

        public bool RemoveStudent(Student student)
        {
            if (student == null)
            {
                return false;
            }

            return _students.Remove(student);
        }

        public List<Student> GetAllStudents()
        {
            return new List<Student>(_students);
        }

        public List<Student> SortByRating()
        {
            var result = new List<Student>(_students);
            result.Sort();
            return result;
        }

        public List<Student> SortByName()
        {
            var result = new List<Student>(_students);
            result.Sort(new StudentNameComparer());
            return result;
        }

        public List<Student> FilterByGroup(string group)
        {
            var result = new List<Student>();

            if (string.IsNullOrWhiteSpace(group))
            {
                return GetAllStudents();
            }

            foreach (var student in _students)
            {
                if (string.Equals(student.Group, group.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(student);
                }
            }

            return result;
        }

        public async Task SaveToFileAsync(string filePath)
        {
            await _fileSaver.SaveAsync(filePath, _students);
        }

        public async Task LoadFromFileAsync(string filePath)
        {
            var loadedStudents = await _fileLoader.LoadAsync<Student>(filePath); // асинхронність
            if (loadedStudents != null)
            {
                _students.Clear();
                foreach (var student in loadedStudents)
                {
                    _students.Add(student);
                }
            }
        }
    }
}
