using KeyboardHandwriting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyboardHandwriting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string CurrentText { get; set; }
        public string UserText { get; set; }

        public HandWriting HandWriting { get; set; }

        public DateTime? StartEnterDate { get; set; }
        public DateTime? EndEnterDate { get; set; }

        public DateTime? KeyPressDate { get; set; }
        public DateTime? KeyDownDate { get; set; }

        public List<double> PausesList { get; set; }
        public List<double> HoldingList { get; set; }
        
        public int OverlappingCount { get; set; }

        public int ErrorCount { get; set; }

        static Brush Good = Brushes.LightGreen;
        static Brush Bad = Brushes.LightSalmon;
        
        private Key PrewKey { get; set; }

        Run Building = null;
        LinkedList<Run> Runs = null;

        public MainWindow()
        {
            InitializeComponent();
            Login.IsEnabled = false;
            NewUser.IsEnabled = false;
            CurrentText = TextCreater.GetText();
            
            CurrentTextTB.TextInput += new TextCompositionEventHandler(CurrentTextTB_PreviewTextInput);
            CurrentTextTB.PreviewKeyDown += new KeyEventHandler(CurrentTextTB_PreviewKeyDown);
            CurrentTextTB.KeyDown += new KeyEventHandler(Text_KeyDown);
            CurrentTextTB.KeyUp += new KeyEventHandler(Text_KeyUp);

            Reset();
            
            PausesList = new List<double>();
            HoldingList = new List<double>();
        }
        
        protected void UpdateParagraph()
        {
            untouched.Text = CurrentText.Substring(Math.Min(CurrentText.Length, UserText.Length));

            PWrapper.Inlines.Clear();

            foreach (Run r in Runs)
            {
                PWrapper.Inlines.Add(r);
            }
            PWrapper.Inlines.Add(Building);
            PWrapper.Inlines.Add(untouched);
            
            TextPointer docStart = CurrentTextTB.Document.ContentStart;
            TextPointer docEnd = CurrentTextTB.Document.ContentEnd;
            int trueLength = docStart.GetOffsetToPosition(docEnd);
            int userStringEndOffset = trueLength - (CurrentText.Length - UserText.Length) - 2;
            TextPointer tp = CurrentTextTB.Document.ContentStart;

            tp = tp.GetPositionAtOffset(userStringEndOffset + 10);  
            if (tp == null)
            {
                tp = docEnd;
            }
            CurrentTextTB.Selection.Select(tp, tp);
        }

        protected void AddCharacter(string c)
        {
            if (UserText.Length == 0)
            {
                StartEnterDate = DateTime.Now;
                KeyPressDate = DateTime.Now;
            }
            else
            {
                PausesList.Add((DateTime.Now - (DateTime)KeyPressDate).TotalMilliseconds);
                KeyPressDate = DateTime.Now;
            }

            UserText += c;

            if (UserText.Length == CurrentText.Length)
            {
                EndEnterDate = DateTime.Now;
                Login.IsEnabled = true;
                NewUser.IsEnabled = true;
            }

            Brush runType = null;


            if (UserText.Length > CurrentText.Length)
            {
                return;
            }
            else
            {
                c = CurrentText[UserText.Length - 1].ToString();
                if (UserText[UserText.Length - 1] == CurrentText[UserText.Length - 1])
                {
                    runType = Good;
                }
                else
                {
                    runType = Bad;
                    ErrorCount++;
                }
            }

            if (runType != null)
            {
                if (Building.Background == runType)
                {
                    Building.Text += c;
                }
                else
                {
                    if (Building.Text.Length > 0)
                    {
                        Runs.AddLast(Building);
                    }
                    Building = new Run(c);
                    Building.Background = runType;
                }
            }

            UpdateParagraph();
        }

        protected void DeleteCharacter()
        {
            if (UserText.Length >= 1)
            {
                UserText = UserText.Substring(0, UserText.Length - 1);

                if (Building.Text.Length == 0 && Runs.Count > 0)
                {
                    Building = Runs.Last.Value;
                    Runs.RemoveLast();
                }

                if (Building.Text.Length > 0)
                {
                    Building.Text = Building.Text.Substring(0, Building.Text.Length - 1);
                }

                UpdateParagraph();
            }
        }

        protected void CurrentTextTB_PreviewTextInput(Object sender, TextCompositionEventArgs e)
        {
            if (e.Text != Convert.ToChar(27).ToString())
            {
                if (e.Text == "\r")
                {
                    AddCharacter("\n");
                }
                else
                {
                    AddCharacter(e.Text);
                }
            }
        }

        protected void CurrentTextTB_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    AddCharacter(" ");
                    break;
                case Key.Tab:
                    AddCharacter("\t");
                    break;
                case Key.Back:
                    DeleteCharacter();
                    break;
            }
        }

        

        private void Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (PrewKey != Key.None && Keyboard.IsKeyDown(PrewKey))
            {
                OverlappingCount++;
            }
            PrewKey = e.Key;
            KeyDownDate = DateTime.Now;
        }

        private void Text_KeyUp(object sender, KeyEventArgs e)
        {
            if (KeyDownDate != null)
            {
                HoldingList.Add((DateTime.Now - (DateTime)KeyDownDate).TotalMilliseconds);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string userName = UserName.Text;
            User currentUser = DatabaseWorker.GetUser(userName);
            if (currentUser == null)
            {
                MessageBox.Show("Имя пользователя не верно или данный пользователь не зарегистрирован в системе");
                return;
            }

            HandWriting = GetHandWriting();
            HandWriting nominalHandWriting = DatabaseWorker.GetUserHandwriting((int)currentUser.Id);

            if (CompareHandWriting(HandWriting, nominalHandWriting))
            {
                MessageBox.Show("Авторизация пройдена успешно");
            }
            else
            {
                MessageBox.Show("Авторизация не пройдена");
            }
        }

        private void NewUser_Click(object sender, RoutedEventArgs e)
        {
            string userName = UserName.Text;
            User currentUser = DatabaseWorker.GetUser(userName);
            if(currentUser != null)
            {
                MessageBox.Show("Пользователь уже зарегистрирован в системе");
                return;
            }
            DatabaseWorker.InsertUser(userName);
            currentUser = DatabaseWorker.GetUser(userName);
            if (currentUser == null)
            {
                MessageBox.Show("Имя пользователя не верно или данный пользователь не зарегистрирован в системе");
                return;
            }

            HandWriting = GetHandWriting(currentUser.Id);
            DatabaseWorker.InserHandwriting(HandWriting);
            MessageBox.Show("Регистрация прошла успешно");
            Reset();
        }

        private HandWriting GetHandWriting(int? userId = null)
        {
            return new HandWriting
            {
                UserId = userId,
                Pauses = Math.Round(PausesList.Average(), 2),
                Holding = Math.Round(HoldingList.Average(), 2),
                ErrorsCount = Math.Round((double)(ErrorCount * 100) / UserText.Length, 2),
                Overlapping = Math.Round((double)(OverlappingCount * 100) / UserText.Length, 2),
                Speed = Math.Round((double)UserText.Length / ((DateTime)EndEnterDate - (DateTime)StartEnterDate).TotalMinutes, 2)
            };
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            CurrentText = TextCreater.GetText();
            CurrentText = CurrentText.Replace("\r", "");

            PausesList = new List<double>();
            HoldingList = new List<double>();
            StartEnterDate = null;
            EndEnterDate = null;
            KeyDownDate = null;
            EndEnterDate = null;
            PrewKey = Key.None;
            ErrorCount = 0;
            OverlappingCount = 0;
            
            Runs = new LinkedList<Run>();
            Building = new Run(string.Empty);
            Building.Background = Good;
            
            UserText = string.Empty;
            
            UpdateParagraph();
        }

        private bool CompareHandWriting(HandWriting currentHandWriting, HandWriting nominalHandWriting)
        {
            bool result = true;

            if(currentHandWriting.Pauses > nominalHandWriting.Pauses + 25 ||
                currentHandWriting.Pauses < nominalHandWriting.Pauses - 25)
            {
                return false;
            }
            if (currentHandWriting.Holding > nominalHandWriting.Holding + 25 ||
                currentHandWriting.Holding < nominalHandWriting.Holding - 25)
            {
                return false;
            }
            if (currentHandWriting.ErrorsCount > nominalHandWriting.ErrorsCount + 5 ||
                currentHandWriting.ErrorsCount < nominalHandWriting.ErrorsCount - 5)
            {
                return false;
            }
            if (currentHandWriting.Overlapping > nominalHandWriting.Overlapping + 10 ||
                currentHandWriting.Overlapping < nominalHandWriting.Overlapping - 10)
            {
                return false;
            }
            if (currentHandWriting.Speed > nominalHandWriting.Speed + 20 ||
                currentHandWriting.Speed < nominalHandWriting.Speed - 20)
            {
                return false;
            }

            return result;
        }
    }
}
