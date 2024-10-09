using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Lab1_OOAP_
{
    public partial class Form1 : Form
    {
        // Список для зберігання всіх намальованих прямокутників до очищення
        private List<ImmutableRectangle> rectangles;
        
        // Список для зберігання очищених прямокутників (для можливості повторного малювання)
        private List<ImmutableRectangle> clearedRectangles;

        // Об'єкт Random для генерації випадкових чисел
        private Random random;

        // Конструктор форми
        public Form1()
        {
            InitializeComponent();

            // Ініціалізація списків та генератора випадкових чисел
            rectangles = new List<ImmutableRectangle>();
            clearedRectangles = new List<ImmutableRectangle>();
            random = new Random();

            // Створення кнопки для малювання випадкових прямокутників
            Button drawButton = new Button { Text = "Draw", Location = new Point(10, 10) };
            drawButton.Click += DrawButton_Click;
            Controls.Add(drawButton);

            // Створення кнопки для очищення прямокутників
            Button clearButton = new Button { Text = "Clear", Location = new Point(110, 10) };
            clearButton.Click += ClearButton_Click;
            Controls.Add(clearButton);

            // Створення кнопки для повторного малювання очищених прямокутників
            Button redrawButton = new Button { Text = "Redraw All", Location = new Point(210, 10) };
            redrawButton.Click += RedrawButton_Click;
            Controls.Add(redrawButton);

            // Створення кнопки для модифікації існуючих прямокутників
            Button modifyButton = new Button { Text = "Modify", Location = new Point(310, 10) };
            modifyButton.Click += ModifyButton_Click;
            Controls.Add(modifyButton);
        }

        // Обробник натискання кнопки "Draw"
        private void DrawButton_Click(object sender, EventArgs e)
        {
            int count = 0; // Для лічильника спроб
            DrawRectangle(count); // Малюємо новий прямокутник
            Invalidate(); // Перемальовуємо форму
        }

        // Обробник натискання кнопки "Clear"
        private void ClearButton_Click(object sender, EventArgs e)
        {
            // Додаємо всі поточні прямокутники в список очищених
            clearedRectangles.Clear(); // Закоментувати якщо потрібно зберігати ВСІ прямокутники
            clearedRectangles.AddRange(rectangles);

            // Очищаємо список прямокутників
            rectangles.Clear();
            Invalidate(); // Перемальовуємо форму
        }

        // Обробник натискання кнопки "Redraw All"
        private void RedrawButton_Click(object sender, EventArgs e)
        {
            // Додаємо всі очищені прямокутники назад у список
            rectangles.AddRange(clearedRectangles);
            clearedRectangles.Clear(); // Очищаємо список очищених прямокутників
            Invalidate(); // Перемальовуємо форму
        }

        // Обробник натискання кнопки "Modify"
        private void ModifyButton_Click(object sender, EventArgs e)
        {
            // Перевіряємо чи є прямокутники для модифікації
            if (rectangles.Count == 0)
            {
                MessageBox.Show("No rectangles to modify!");
                return;
            }

            // Відкриваємо форму для введення нових параметрів прямокутника
            using (var inputForm = new InputForm(rectangles.Count))
            {
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    int index = inputForm.SelectedIndex;
                    int newX = inputForm.NewX;
                    int newY = inputForm.NewY;
                    int newWidth = inputForm.NewWidth;
                    int newHeight = inputForm.NewHeight;

                    // Перевіряємо, чи новий прямокутник виходить за межі форми
                    if (newX + newWidth > ClientSize.Width) newWidth = ClientSize.Width - newX;
                    if (newY + newHeight > ClientSize.Height) newHeight = ClientSize.Height - newY;

                    // Створюємо новий прямокутник
                    var newRect = new ImmutableRectangle(newX, newY, newWidth, newHeight);

                    // Якщо нема перетину з існуючими прямокутниками
                    if (!IsOverlapping(newRect))
                    {
                        rectangles.Add(newRect); // Додаємо новий прямокутник до списку
                        Invalidate(); // Перемальовуємо форму
                        MessageBox.Show($"Rectangle at index {index} was modified.");
                    }
                    // Якщо є перетин то повторюємо спробу
                    else
                    {
                        MessageBox.Show("A rectangle with these parameters intersects with others! Try again!");
                        ModifyButton_Click(sender, e);
                    }
                }
            }
        }

        // Метод для малювання випадкового прямокутника
        private void DrawRectangle(int count)
        {
            count++; // Лічильник спроб

            int x = random.Next(0, ClientSize.Width);
            int y = random.Next(0, ClientSize.Height);
            int width = random.Next(20, 100);
            int height = random.Next(20, 100);

            // Перевірка на вихід за межі форми
            if (x + width > ClientSize.Width) width = ClientSize.Width - x;
            if (y + height > ClientSize.Height) height = ClientSize.Height - y;

            // Створюємо новий прямокутник
            var newRect = new ImmutableRectangle(x, y, width, height);

            // Перевірка на перетин з існуючими прямокутниками
            if (!IsOverlapping(newRect))
            {
                rectangles.Add(newRect); // Додаємо прямокутник до списку, якщо немає перетинів
                Invalidate(); // Перемальовуємо форму
            }
            else if (count < 100)
            {
                DrawRectangle(count); // Якщо є перетин і ще є спроби, повторюємо метод
            }
            // Якщо використано 100 спроб скоріше за все місця вже точно нема
            else
            {
                MessageBox.Show($"The program was unable to find space for the new rectangle after {count} attempts!");
            }
        }

        // Метод для перевірки перетинів прямокутників
        private bool IsOverlapping(ImmutableRectangle newRect)
        {
            foreach (var rect in rectangles)
            {
                // Перевірка перетинів
                if (newRect.ToRectangle().IntersectsWith(rect.ToRectangle()))
                {
                    return true; // Прямокутники перетинаються
                }
            }
            return false; // Прямокутники не перетинаються
        }

        // Перевизначення методу OnPaint для малювання прямокутників на формі
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Малюємо всі прямокутники з списку
            for (int i = 0; i < rectangles.Count; i++)
            {
                var rect = rectangles[i];

                // Малюємо контур прямокутника
                using (var pen = new Pen(Color.Black))
                {
                    e.Graphics.DrawRectangle(pen, rect.ToRectangle());
                }

                // Малюємо індекс прямокутника всередині нього
                using (var font = new Font("Arial", 10))
                using (var brush = new SolidBrush(Color.Red))
                {
                    var text = i.ToString();
                    var textSize = e.Graphics.MeasureString(text, font);
                    var textX = rect.X + (rect.Width - textSize.Width) / 2;
                    var textY = rect.Y + (rect.Height - textSize.Height) / 2;

                    e.Graphics.DrawString(text, font, brush, textX, textY);
                }
            }
        }
    }

    // Клас для представлення прямокутника з незмінними властивостями
    public class ImmutableRectangle
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public ImmutableRectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        // Метод для конвертації в Rectangle для використання в графічних операціях
        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }
    }

    // Форма для модифікації прямокутників
    public class InputForm : Form
    {
        private NumericUpDown indexInput;
        private NumericUpDown xInput;
        private NumericUpDown yInput;
        private NumericUpDown widthInput;
        private NumericUpDown heightInput;
        private Button okButton;
        private Button cancelButton;

        // Властивості для доступу до введених даних
        public int SelectedIndex => (int)indexInput.Value;
        public int NewX => (int)xInput.Value;
        public int NewY => (int)yInput.Value;
        public int NewWidth => (int)widthInput.Value;
        public int NewHeight => (int)heightInput.Value;

        public InputForm(int rectangleCount)
        {
            Text = "Modify Rectangle";
            Width = 300;
            Height = 250;

            // Ініціалізація полів для вводу даних
            Label indexLabel = new Label { Text = "Index:", Location = new Point(10, 10), Width = 100 };
            indexInput = new NumericUpDown { Location = new Point(120, 10), Minimum = 0, Maximum = rectangleCount - 1 };

            Label xLabel = new Label { Text = "X:", Location = new Point(10, 40), Width = 100 };
            xInput = new NumericUpDown { Location = new Point(120, 40), Minimum = 0, Maximum = 1000 };

            Label yLabel = new Label { Text = "Y:", Location = new Point(10, 70), Width = 100 };
            yInput = new NumericUpDown { Location = new Point(120, 70), Minimum = 0, Maximum = 1000 };

            Label widthLabel = new Label { Text = "Width:", Location = new Point(10, 100), Width = 100 };
            widthInput = new NumericUpDown { Location = new Point(120, 100), Minimum = 20, Maximum = 100 };

            Label heightLabel = new Label { Text = "Height:", Location = new Point(10, 130), Width = 100 };
            heightInput = new NumericUpDown { Location = new Point(120, 130), Minimum = 20, Maximum = 100 };

            // Кнопка OK для підтвердження змін
            okButton = new Button { Text = "OK", Location = new Point(50, 170), DialogResult = DialogResult.OK };
            // Кнопка Cancel для відміни змін
            cancelButton = new Button { Text = "Cancel", Location = new Point(150, 170), DialogResult = DialogResult.Cancel };

            // Додавання кнопок і полів на форму
            Controls.Add(indexLabel);
            Controls.Add(indexInput);
            Controls.Add(xLabel);
            Controls.Add(xInput);
            Controls.Add(yLabel);
            Controls.Add(yInput);
            Controls.Add(widthLabel);
            Controls.Add(widthInput);
            Controls.Add(heightLabel);
            Controls.Add(heightInput);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

            // Налаштування кнопок для підтвердження і відміни
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }
    }
}