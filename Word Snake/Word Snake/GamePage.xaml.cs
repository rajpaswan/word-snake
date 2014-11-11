using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Word_Snake
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        private Block[][] blocks;
        private int row_size = 30;
        private int column_size = 20;
        private Brush wall_brush = new SolidColorBrush(Colors.DarkGray);
        private Brush empty_brush = new SolidColorBrush(Colors.Transparent);
        private Brush snake_brush = new SolidColorBrush(Colors.GreenYellow);
        private Brush fruit_brush = new SolidColorBrush(Colors.OrangeRed);
        private Brush black_brush = new SolidColorBrush(Colors.Black);

        private int head_x;
        private int head_y;
        //private int bottom;
        private int dir_x;
        private int dir_y;

        private List<int> body_x;
        private List<int> body_y;
        private List<int> fruit_x;
        private List<int> fruit_y;
        private List<String> fruits;
        private int food;
        private List<String> stomach;
        private List<String> eaten;

        List<string> allWords;

        private int score = 0;
        private String buffer = "";
        private Random random = new Random(System.DateTime.Now.Second);
        //private int length;

        DispatcherTimer timer;
        GestureRecognizer gestureRecognizer = new GestureRecognizer();

        public GamePage()
        {
            this.InitializeComponent();

            this.InitGame();

            this.gestureRecognizer.GestureSettings =
            Windows.UI.Input.GestureSettings.Tap |
            Windows.UI.Input.GestureSettings.ManipulationTranslateX |
            Windows.UI.Input.GestureSettings.ManipulationTranslateY;
            //Windows.UI.Input.GestureSettings.Hold | //hold must be set in order to recognize the press & hold gesture 
            //Windows.UI.Input.GestureSettings.RightTap |
            //Windows.UI.Input.GestureSettings.ManipulationRotate |
            //Windows.UI.Input.GestureSettings.ManipulationScale |
            //Windows.UI.Input.GestureSettings.ManipulationTranslateInertia |
            //Windows.UI.Input.GestureSettings.ManipulationRotateInertia |
            //Windows.UI.Input.GestureSettings.ManipulationMultipleFingerPanning | //reduces zoom jitter when panning with multiple fingers 
            //Windows.UI.Input.GestureSettings.ManipulationScaleInertia;

            // Set up pointer event handlers. These receive input events that are used by the gesture recognizer. 
            this.layer_grid.PointerCanceled += OnPointerCanceled;
            this.layer_grid.PointerPressed += OnPointerPressed;
            this.layer_grid.PointerReleased += OnPointerReleased;
            this.layer_grid.PointerMoved += OnPointerMoved;

            // Set up event handlers to respond to gesture recognizer output 
            this.gestureRecognizer.Tapped += OnTapped;
            //this.gestureRecognizer.RightTapped += OnRightTapped;
            //this.gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            //this.gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            this.gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;

            //Set up the dispatcher timer
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(200);
            this.timer.Tick += timer_Tick;
        }

        private void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            double vx = args.Velocities.Linear.X;
            double vy = args.Velocities.Linear.Y;

            int dx = 0;
            int dy = 0;

            if (Math.Abs(vx) > Math.Abs(vy))
                dx = vx > 0 ? 1 : -1;
            else
                dy = vy > 0 ? 1 : -1;

            if (dir_x == 0 && dx != 0)
            {
                dir_x = dx;
                dir_y = 0;
            }
            else if (dir_y == 0 && dy != 0)
            {
                dir_x = 0;
                dir_y = dy;
            }
        }

        private void OnTapped(GestureRecognizer sender, TappedEventArgs args)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                title_block.Text = "PAUSED";
                text_block.Text = "tap to continue";
                layer_grid.Children.Add(msg_group);
                //msg_group.Visibility = Windows.UI.Xaml.Visibility.Visible;
                //layer_grid.Background = empty_brush;
            }
            else
            {
                layer_grid.Children.Remove(msg_group);
                //msg_group.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                layer_grid.Background = empty_brush;
                timer.Start();
            }
        }

        void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            // Route the events to the gesture recognizer 
            // The points are in the main_grid frame of the canvas that contains the rectangle element. 
            this.gestureRecognizer.ProcessDownEvent(args.GetCurrentPoint(this.main_grid));
            // Set the pointer capture to the element being interacted with 
            this.layer_grid.CapturePointer(args.Pointer);
            // Mark the event handled to prevent execution of default handlers 
            args.Handled = true;
        }

        void OnPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            this.gestureRecognizer.ProcessUpEvent(args.GetCurrentPoint(this.main_grid));
            args.Handled = true;
        }

        void OnPointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            this.gestureRecognizer.CompleteGesture();
            args.Handled = true;
        }

        void OnPointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            this.gestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(this.main_grid));
            args.Handled = true;
        }

        /// <summary>
        /// timer job
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, object e)
        {
            // check for conditions
            int pos = -1;
            int max = 0;

            for (int i = 0; i < fruit_x.Count; ++i)
            {
                if (head_x == fruit_x[i] && head_y == fruit_y[i])
                {
                    pos = i;
                    break;
                }
            }

            if (pos >= 0)
            {
                food = pos;
                GenerateFruit();
                score++;
            }

            // process food
            if (fruit_x[food] == head_x && fruit_y[food] == head_y)
            {
                // play pick sound effect
                pick_media.Play();

                stomach.Add(fruits[food]);
                if (stomach.Count > 1)
                {
                    body_x.Add(body_x[body_x.Count - 1]);
                    body_y.Add(body_y[body_y.Count - 1]);
                }
                
                //remove fruit
                fruit_x.RemoveAt(food);
                fruit_y.RemoveAt(food);
                fruits.RemoveAt(food);
                
                //check for word match
                string sequence1 = "";
                string sequence2 = "";
                foreach (String ch in stomach)
                {
                    sequence1 = ch + sequence1;
                    sequence2 = sequence2 + ch;
                }

                String match = "";

                foreach (String word in allWords)
                {
                    if ((eaten == null || !eaten.Contains(word)))
                    {
                        if (sequence1.Contains(word) || sequence2.Contains(word))
                        {
                            score += word.Length;
                            match += word + " ";
                            eaten.Add(word);
                            if (word.Length > max)
                                max = word.Length;
                        }
                    }
                }

                // show matched words
                if (max > 0)
                {
                    info_block.Text = match;
                    pop_media.Play();
                }
 
                // digest the matched word
                stomach.RemoveRange(stomach.Count - max, max);
            }

            // erase whole body
            for (int i = 0; i < body_x.Count; ++i)
            {
                blocks[body_x[i]][body_y[i]].Text = "";
                blocks[body_x[i]][body_y[i]].Fill = empty_brush;
                blocks[body_x[i]][body_y[i]].Stroke = empty_brush;
            }

            // cut tail if words matched
            if (max > 0)
            {
                body_x.RemoveRange(body_x.Count - max, max);
                body_y.RemoveRange(body_y.Count - max, max);
            }

            // maintain min length
            if (body_x.Count == 0)
            {
                body_x.Add(0);
                body_y.Add(0);
            }
            
            // shift all body
            for (int i = body_x.Count - 1; i > 0; --i)
            {
                body_x[i] = body_x[i - 1];
                body_y[i] = body_y[i - 1];
            }

            // shift head to neck
            body_x[0] = head_x;
            body_y[0] = head_y;

            // move head forward
            head_x = head_x + dir_x;
            head_y = head_y + dir_y;

            //walls
            if (head_x == 0)
                head_x = column_size - 2;
            if (head_x == column_size - 1)
                head_x = 1;
            if (head_y == 0)
                head_y = row_size - 2;
            if (head_y == row_size - 1)
                head_y = 1;

            // render whole snake
            for (int i = 0; i < body_x.Count; ++i)
            {
                //blocks[body_x[i]][body_y[i]].Text = "";
                blocks[body_x[i]][body_y[i]].Fill = snake_brush;
                blocks[body_x[i]][body_y[i]].Stroke = empty_brush;
            }

            // render new head
            //blocks[head_x][head_y].Text = "";
            //blocks[head_x][head_y].TextColor = fruit_brush;
            //blocks[head_x][head_y].Fill = empty_brush;
            blocks[head_x][head_y].Stroke = snake_brush;

            for (int i = 0; i < stomach.Count; ++i)
            {
                blocks[body_x[body_x.Count - i - 1]][body_y[body_y.Count - i - 1]].Text = stomach[i];
                blocks[body_x[body_x.Count - i - 1]][body_y[body_y.Count - i - 1]].TextColor = black_brush;
            }

            UpdateScore();
        }

        private void UpdateScore()
        {
            score_block.Text = "Words : " + eaten.Count + "     Score : " + score;
            if (buffer.Length == 0 && fruits.Count == 0)
            {
                timer.Stop();
                MessageDialog md = new MessageDialog("Your score is " + score, "Game Over");
                md.ShowAsync();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void InitGame()
        {
            // get words
            allWords = await new WordList().GetWords(4, 3000);

            if (allWords == null)
            {
                MessageDialog md = new MessageDialog("cannot load words!", "Error");
                await md.ShowAsync();
                return;
            }

            foreach (String word in allWords)
            {
                buffer += word;
            }

            // init snake
            head_x = 10;
            head_y = 14;

            body_x = new List<int>();
            body_y = new List<int>();

            body_x.Add(10);
            body_y.Add(15);
            //body_x.Add(6);
            //body_y.Add(12);

            //bottom = 0;

            dir_x = 0;
            dir_y = -1;

            stomach = new List<String>();
            eaten = new List<string>();
            //length = 1;

            Thickness margin = new Thickness(0);

            blocks = new Block[column_size][];

            for (int i = 0; i < column_size; ++i)
            {
                blocks[i] = new Block[row_size];

                for (int j = 0; j < row_size; ++j)
                {
                    blocks[i][j] = new Block();

                    blocks[i][j].HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                    blocks[i][j].VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;

                    margin.Left = 20 * i;
                    margin.Top = 20 * j;
                    blocks[i][j].Margin = margin;

                    if (i == 0 || j == 0 || i == column_size - 1 || j == row_size - 1)
                    {
                        blocks[i][j].Fill = wall_brush;
                    }

                    block_grid.Children.Add(blocks[i][j]);
                }
            }

            // head
            blocks[head_x][head_y].Fill = empty_brush;
            blocks[head_x][head_y].Stroke = snake_brush;
            // body
            blocks[body_x[0]][body_y[0]].Fill = snake_brush;
            blocks[body_x[0]][body_y[0]].Stroke = empty_brush;
            //blocks[body_x[1]][body_y[1]].Fill = snake_brush;
            //blocks[body_x[1]][body_y[1]].Stroke = empty_brush;

            GenerateFruit();
            GenerateFruit();
            GenerateFruit();
        }

        private void GenerateFruit()
        {
            String text = GetNextLetter();

            if (text == "")
                return;


            // generatiing a random postion for new fruit
            bool free;
            int px, py;

            do
            {
                free = true;
                px = 1 + random.Next(column_size - 2);
                py = 1 + random.Next(row_size - 2);

                for (int i = 0; i < body_x.Count; ++i)
                {
                    if (px == body_x[i] && py == body_y[i])
                    {
                        free = false;
                        break;
                    }
                }

                if (px == head_x && py == head_y)
                    free = false;

            } while (!free);



            if (fruit_x == null || fruit_y == null || fruits == null)
            {
                fruit_x = new List<int>();
                fruit_y = new List<int>();
                fruits = new List<string>();
            }

            fruit_x.Add(px);
            fruit_y.Add(py);
            fruits.Add(text);

            blocks[px][py].Text = text; // get next letter
            //blocks[px][py].Stroke = fruit_brush;
            blocks[px][py].Fill = fruit_brush;
            blocks[px][py].TextColor = black_brush;

        }

        private String GetNextLetter()
        {
            if (buffer.Length > 0)
            {
                int index = random.Next(buffer.Length);
                char result = buffer[index];
                buffer = buffer.Remove(index, 1);
                return "" + result;
            }
            else
                return "";
        }
    }
}