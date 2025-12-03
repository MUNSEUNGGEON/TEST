using System.Windows;
using System.Windows.Controls;

namespace Novacos_AIManager.View
{
    public partial class UserEditDialog : Window
    {
        // ───────────────────────── 속성 ─────────────────────────
        public string UserId => IdTextBox.Text.Trim();
        public string Password => PasswordBox.Password;
        public string PasswordConfirm => PasswordConfirmBox.Password;
        public string Email => EmailTextBox.Text.Trim();
        public string UserName => UserNameTextBox.Text.Trim();

        public string UserType
        {
            get
            {
                if (UserTypeComboBox.SelectedItem is ComboBoxItem item)
                    return (item.Tag ?? item.Content)?.ToString() ?? string.Empty;
                return string.Empty;
            }
        }

        public string Department
        {
            get
            {
                if (DepartmentComboBox.SelectedItem is ComboBoxItem item)
                    return (item.Tag ?? item.Content)?.ToString() ?? string.Empty;
                return string.Empty;
            }
        }

        public string Position
        {
            get
            {
                if (PositionComboBox.SelectedItem is ComboBoxItem item)
                    return (item.Tag ?? item.Content)?.ToString() ?? string.Empty;
                return string.Empty;
            }
        }

        // ───────────────────────── 생성자 ─────────────────────────
        public UserEditDialog()
        {
            InitializeComponent();
        }

        // ───────────────────────── 버튼 핸들러 ─────────────────────────
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 필수값 체크
            if (string.IsNullOrWhiteSpace(UserId))
            {
                MessageBox.Show("아이디를 입력하세요.", "입력 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("비밀번호를 입력하세요.", "입력 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordConfirm))
            {
                MessageBox.Show("비밀번호 확인을 입력하세요.", "입력 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Password != PasswordConfirm)
            {
                MessageBox.Show("비밀번호와 비밀번호 확인이 일치하지 않습니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("이메일을 입력하세요.", "입력 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(UserName))
            {
                MessageBox.Show("사용자 이름을 입력하세요.", "입력 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(UserType) || UserType == "선택")
            {
                MessageBox.Show("사용자 유형을 선택하세요.", "입력 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Department) || Department == "선택")
            {
                MessageBox.Show("소속을 선택하세요.", "입력 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Position) || Position == "선택")
            {
                MessageBox.Show("직책을 선택하세요.", "입력 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            

            // 여기까지 통과하면 정상 입력
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
