using System.ComponentModel;

namespace Novacos_AIManager.Model
{
    public class UserInfoModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        private string? _userType;
        public string? UserType
        {
            get => _userType;
            set
            {
                _userType = value;
                OnPropertyChanged(nameof(UserType));
            }
        }

        private string? _department;
        public string? Department
        {
            get => _department;
            set
            {
                _department = value;
                OnPropertyChanged(nameof(Department));
            }
        }

        private string? _position;
        public string? Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        public string? CreatedAt { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            private set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }

        private string? _editDepartment;
        public string? EditDepartment
        {
            get => _editDepartment;
            set
            {
                _editDepartment = value;
                OnPropertyChanged(nameof(EditDepartment));
            }
        }

        private string? _editPosition;
        public string? EditPosition
        {
            get => _editPosition;
            set
            {
                _editPosition = value;
                OnPropertyChanged(nameof(EditPosition));
            }
        }

        private string? _editUserType;
        public string? EditUserType
        {
            get => _editUserType;
            set
            {
                _editUserType = value;
                OnPropertyChanged(nameof(EditUserType));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void BeginEdit()
        {
            EditDepartment = Department;
            EditPosition = Position;
            EditUserType = UserType;
            IsEditing = true;
        }

        public void CommitEdit()
        {
            Department = EditDepartment;
            Position = EditPosition;
            UserType = EditUserType;
            IsEditing = false;
        }

        public void CancelEdit()
        {
            EditDepartment = Department;
            EditPosition = Position;
            EditUserType = UserType;
            IsEditing = false;
        }
    }
}
