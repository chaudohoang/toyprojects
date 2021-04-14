// -----------------------------------------------------------------------------------
// Program  : To implement the Sliding Puzzle game
// Code By  : Tushar Agarwal
// Email    : tushar_ag@yahoo.com, a_tushar@hotmail.com, tushar_bbit@rediffmail.com
//
// © CopyRight Reserved: This code is protected against copyright.
// The use, reproduction, change of this code in any form or by any means without the 
// prior permission of the author will be punishable.
// ----------------------------------------------------------------------------------

// If there are any errors please do tell me 
// For any Help, suggestions, comments please feel free to mail me or call me at
// (+91) 98680 52445

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb ;


namespace Tushar.SlidingPuzzle
{
	/// <summary>
	/// Summary description for Main Form.
	/// </summary>


	public class formMain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Button button9;
		private System.Windows.Forms.Button button10;
		private System.Windows.Forms.Button button11;
		private System.Windows.Forms.Button button12;
		private System.Windows.Forms.Button button13;
		private System.Windows.Forms.Button button14;
		private System.Windows.Forms.Button button15;
		private System.Drawing.Point EmptyPoint;
		// ArrayList alAllButtons to store all the Buttons on the form
		private ArrayList alAllButtons;
		// ArrayList alSmallImages to store small pictures to be displayed on the buttons
		private ArrayList ilSmallImages;
		// This is the main Picture which is to be formed
		private Bitmap MainBitmap;
		
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.Button buttonShowHide;
		private System.Windows.Forms.Button buttonReStart;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem miFileNewGame;
		private System.Windows.Forms.Button buttonChangeImage;
		private System.Windows.Forms.Button buttonRefresh;
		private System.Windows.Forms.MenuItem miFile;
		private System.Windows.Forms.MenuItem miMode;
		private System.Windows.Forms.MenuItem miFileModeNumber;
		private System.Windows.Forms.MenuItem miFileModePicture;
		private System.Windows.Forms.MenuItem miFileModeNumPic;
		private System.Windows.Forms.MenuItem miFileLoadPicture;
		private System.Windows.Forms.MenuItem miFileExit;
		private System.Windows.Forms.MenuItem miHelpAbout;
        private IContainer components;

        public formMain()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		
			// This point keeps track of the empty point where there is no button
			// a place where a button move can be made
			EmptyPoint = new Point ();
			
			// Create a ArrayList alAllButtons to store the buttons
			alAllButtons = new ArrayList ();
			// Add all the buttons present on the panel to the ArrayList
			foreach(Button b in panel1.Controls )
				alAllButtons.Add (b);

			// Try loading the Default picture from the default path
			try
			{
				MainBitmap = (Bitmap)Bitmap.FromFile (Application.StartupPath +"\\game.jpg");
				MessageBox.Show (Application.StartupPath.ToString ()); 
			}
			catch (Exception ex)
			{
				// If there is an error then display the error message.
				MessageBox.Show (ex.Message.ToString ());
			}

			// Initialize Empty Point to the down right most corner of the panel
			Point p = new Point (320,320);
			EmptyPoint.X = 240;
			EmptyPoint.Y = 240;

			// Initialize the Game Mode to the Picture Mode
			miFileModeNumber.Checked = false;
			miFileModePicture.Checked = true;
			miFileModeNumPic.Checked = false;
			HideNumbersFromButtons();
			
			
			// This function does the extra initilization needed by the game
			NewGame();	

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button15 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonShowHide = new System.Windows.Forms.Button();
            this.buttonReStart = new System.Windows.Forms.Button();
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.miFile = new System.Windows.Forms.MenuItem();
            this.miFileNewGame = new System.Windows.Forms.MenuItem();
            this.miFileLoadPicture = new System.Windows.Forms.MenuItem();
            this.miMode = new System.Windows.Forms.MenuItem();
            this.miFileModeNumber = new System.Windows.Forms.MenuItem();
            this.miFileModePicture = new System.Windows.Forms.MenuItem();
            this.miFileModeNumPic = new System.Windows.Forms.MenuItem();
            this.miFileExit = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.miHelpAbout = new System.Windows.Forms.MenuItem();
            this.buttonChangeImage = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button15);
            this.panel1.Controls.Add(this.button14);
            this.panel1.Controls.Add(this.button13);
            this.panel1.Controls.Add(this.button12);
            this.panel1.Controls.Add(this.button11);
            this.panel1.Controls.Add(this.button10);
            this.panel1.Controls.Add(this.button9);
            this.panel1.Controls.Add(this.button8);
            this.panel1.Controls.Add(this.button7);
            this.panel1.Controls.Add(this.button6);
            this.panel1.Controls.Add(this.button5);
            this.panel1.Controls.Add(this.button4);
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Location = new System.Drawing.Point(20, 10);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(320, 320);
            this.panel1.TabIndex = 0;
            // 
            // button15
            // 
            this.button15.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button15.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button15.ForeColor = System.Drawing.SystemColors.Control;
            this.button15.Location = new System.Drawing.Point(160, 240);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(80, 80);
            this.button15.TabIndex = 14;
            this.button15.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button14
            // 
            this.button14.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button14.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button14.ForeColor = System.Drawing.SystemColors.Control;
            this.button14.Location = new System.Drawing.Point(80, 240);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(80, 80);
            this.button14.TabIndex = 13;
            this.button14.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button13
            // 
            this.button13.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button13.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button13.ForeColor = System.Drawing.SystemColors.Control;
            this.button13.Location = new System.Drawing.Point(0, 240);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(80, 80);
            this.button13.TabIndex = 12;
            this.button13.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button12
            // 
            this.button12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button12.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button12.ForeColor = System.Drawing.SystemColors.Control;
            this.button12.Location = new System.Drawing.Point(240, 160);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(80, 80);
            this.button12.TabIndex = 11;
            this.button12.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button11
            // 
            this.button11.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button11.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button11.ForeColor = System.Drawing.SystemColors.Control;
            this.button11.Location = new System.Drawing.Point(160, 160);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(80, 80);
            this.button11.TabIndex = 10;
            this.button11.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button10
            // 
            this.button10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button10.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button10.ForeColor = System.Drawing.SystemColors.Control;
            this.button10.Location = new System.Drawing.Point(80, 160);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(80, 80);
            this.button10.TabIndex = 9;
            this.button10.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button9
            // 
            this.button9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button9.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button9.ForeColor = System.Drawing.SystemColors.Control;
            this.button9.Location = new System.Drawing.Point(0, 160);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(80, 80);
            this.button9.TabIndex = 8;
            this.button9.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button8
            // 
            this.button8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button8.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button8.ForeColor = System.Drawing.SystemColors.Control;
            this.button8.Location = new System.Drawing.Point(240, 80);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(80, 80);
            this.button8.TabIndex = 7;
            this.button8.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button7
            // 
            this.button7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button7.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button7.ForeColor = System.Drawing.SystemColors.Control;
            this.button7.Location = new System.Drawing.Point(160, 80);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(80, 80);
            this.button7.TabIndex = 6;
            this.button7.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button6
            // 
            this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button6.ForeColor = System.Drawing.SystemColors.Control;
            this.button6.Location = new System.Drawing.Point(80, 80);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(80, 80);
            this.button6.TabIndex = 5;
            this.button6.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button5
            // 
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.ForeColor = System.Drawing.SystemColors.Control;
            this.button5.Location = new System.Drawing.Point(0, 80);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(80, 80);
            this.button5.TabIndex = 4;
            this.button5.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.ForeColor = System.Drawing.SystemColors.Control;
            this.button4.Location = new System.Drawing.Point(240, 0);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(80, 80);
            this.button4.TabIndex = 3;
            this.button4.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.ForeColor = System.Drawing.SystemColors.Control;
            this.button3.Location = new System.Drawing.Point(160, 0);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(80, 80);
            this.button3.TabIndex = 2;
            this.button3.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.SystemColors.Control;
            this.button2.Location = new System.Drawing.Point(80, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(80, 80);
            this.button2.TabIndex = 1;
            this.button2.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.SystemColors.Control;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 80);
            this.button1.TabIndex = 0;
            this.button1.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // buttonShowHide
            // 
            this.buttonShowHide.Location = new System.Drawing.Point(240, 344);
            this.buttonShowHide.Name = "buttonShowHide";
            this.buttonShowHide.Size = new System.Drawing.Size(104, 24);
            this.buttonShowHide.TabIndex = 1;
            this.buttonShowHide.Text = "Show Image";
            this.buttonShowHide.Click += new System.EventHandler(this.buttonShowHideOnClick);
            // 
            // buttonReStart
            // 
            this.buttonReStart.Location = new System.Drawing.Point(16, 344);
            this.buttonReStart.Name = "buttonReStart";
            this.buttonReStart.Size = new System.Drawing.Size(88, 24);
            this.buttonReStart.TabIndex = 2;
            this.buttonReStart.Text = "ReStart";
            this.buttonReStart.Click += new System.EventHandler(this.OnButtonReStartClick);
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miFile,
            this.menuItem7});
            // 
            // miFile
            // 
            this.miFile.Index = 0;
            this.miFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miFileNewGame,
            this.miFileLoadPicture,
            this.miMode,
            this.miFileExit});
            this.miFile.Text = "File";
            // 
            // miFileNewGame
            // 
            this.miFileNewGame.Index = 0;
            this.miFileNewGame.Text = "New Game";
            this.miFileNewGame.Click += new System.EventHandler(this.OnmiFileNewGameClick);
            // 
            // miFileLoadPicture
            // 
            this.miFileLoadPicture.Index = 1;
            this.miFileLoadPicture.Text = "Change Picture";
            this.miFileLoadPicture.Click += new System.EventHandler(this.OnmiFileChangePicture);
            // 
            // miMode
            // 
            this.miMode.Index = 2;
            this.miMode.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miFileModeNumber,
            this.miFileModePicture,
            this.miFileModeNumPic});
            this.miMode.Text = "Mode";
            // 
            // miFileModeNumber
            // 
            this.miFileModeNumber.Checked = true;
            this.miFileModeNumber.Index = 0;
            this.miFileModeNumber.RadioCheck = true;
            this.miFileModeNumber.Text = "Number Mode";
            this.miFileModeNumber.Click += new System.EventHandler(this.OnmiFileNumberModeClick);
            // 
            // miFileModePicture
            // 
            this.miFileModePicture.Index = 1;
            this.miFileModePicture.RadioCheck = true;
            this.miFileModePicture.Text = "Picture Mode";
            this.miFileModePicture.Click += new System.EventHandler(this.OnmiFileModePictureClick);
            // 
            // miFileModeNumPic
            // 
            this.miFileModeNumPic.Index = 2;
            this.miFileModeNumPic.Text = "Number and Picture Mode";
            this.miFileModeNumPic.Click += new System.EventHandler(this.OnmiFileModeNumPic);
            // 
            // miFileExit
            // 
            this.miFileExit.Index = 3;
            this.miFileExit.Text = "E&xit";
            this.miFileExit.Click += new System.EventHandler(this.OnFileExitClick);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 1;
            this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem8,
            this.miHelpAbout});
            this.menuItem7.Text = "Help";
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 0;
            this.menuItem8.Text = "Help";
            // 
            // miHelpAbout
            // 
            this.miHelpAbout.Index = 1;
            this.miHelpAbout.Text = "About";
            this.miHelpAbout.Click += new System.EventHandler(this.OnmiHelpAboutClick);
            // 
            // buttonChangeImage
            // 
            this.buttonChangeImage.Location = new System.Drawing.Point(112, 344);
            this.buttonChangeImage.Name = "buttonChangeImage";
            this.buttonChangeImage.Size = new System.Drawing.Size(112, 24);
            this.buttonChangeImage.TabIndex = 3;
            this.buttonChangeImage.Text = "&Change Image";
            this.buttonChangeImage.Click += new System.EventHandler(this.OnbuttonChangeImageClick);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(488, 344);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(96, 23);
            this.buttonRefresh.TabIndex = 4;
            this.buttonRefresh.Text = "Re&fresh";
            this.buttonRefresh.Visible = false;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // formMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(360, 385);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonChangeImage);
            this.Controls.Add(this.buttonReStart);
            this.Controls.Add(this.buttonShowHide);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Menu = this.mainMenu;
            this.Name = "formMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Sliding Tile Game - Made by Tushar Agarwal";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new formMain());
		}



		// This function is called whenever a user wants to see or hide the Image
		// If the image is already being displayed then hide it otherwise show it
		// This works as a toggle function to Show/Hide the Image
		private void buttonShowHideOnClick(object sender, System.EventArgs e)
		{
			Button tempBtn = (Button)sender;

			// If the image is not shown, Show it.
			if (tempBtn.Text == "Show Image")
			{
				this.Size = new Size (720,450);
				Graphics g = this.CreateGraphics ();
				g.DrawImage (MainBitmap,375,10,MainBitmap.Width ,MainBitmap.Height   );
				g.DrawString ("Made by Tushar Agarwal",this.Font ,Brushes.MistyRose ,375,20);   

				g.Dispose ();
				tempBtn.Text = "Hide Image";
				buttonRefresh.Visible = true;
			}
            // If the image is already displayed and the user wants' to Hide it.    
			else 
			{
				this.Size = new Size (375,450);
				tempBtn.Text = "Show Image";
				buttonRefresh.Visible = false;
			}
		}

		// This is the function which moves the button to the Empty places
		// This function takes a button as a parameter
		private void MoveButton(Button ClickedButton )
		{
		
			// Proceed with	the	function only if the button	is either horizontally or
			// vertically inline with the Empty	Point			
			if(EmptyPoint.X	== ClickedButton.Location.X	|| EmptyPoint.Y	== ClickedButton.Location.Y)
			{
				// After the button	will be	moved the new Empty	Point is gonna change
				// So store	the	current	position of	the	Button in a	tempPoint
				Point tempEmptyPoint =	new	Point ();
				tempEmptyPoint = ClickedButton.Location;

				// Find out in which direction we have to move the Button
				int	d =	Direction (ClickedButton.Location ,EmptyPoint);

				// Follow this procedure if we have to move the button Up or Down
				// i.e. the button is vertically inline with the Empty Point
				if(EmptyPoint.X	== ClickedButton.Location.X	)
				{
					// There could be some other buttons that are in b/w the clicked
					// button and the EmptyPoint. So we'll have to shift all the buttons
					foreach(Button bx in panel1.Controls )
					{
						if (((bx.Location.Y	>= ClickedButton.Location.Y) 
							&& (bx.Location.Y <	EmptyPoint.Y ) &&(bx.Location.X	== EmptyPoint.X	)) || ((bx.Location.Y <= ClickedButton.Location.Y) 
							&& (bx.Location.Y >	EmptyPoint.Y )&&(bx.Location.X == EmptyPoint.X )))
						{
							switch (d)
							{
								case 1:	Functions.MoveUp(bx);
									break;
								case 3:	Functions.MoveDown(bx);
									break;
							}
						}
				
					}
				}

				// Follow this procedure if we have to move the button Left or Right
				// i.e. the Clicked button is horizontally inline with the Empty Space
				if(EmptyPoint.Y	== ClickedButton.Location.Y	)
				{
					// There could be some other buttons that are in b/w the clicked
					// button and the EmptyPoint. So we'll have to shift all the buttons
					foreach(Button bx in panel1.Controls )
					{
						if (((bx.Location.X	>= ClickedButton.Location.X) 
							&& (bx.Location.X <	EmptyPoint.X ) && (bx.Location.Y ==	EmptyPoint.Y  )) ||	((bx.Location.X	<= ClickedButton.Location.X) 
							&& (bx.Location.X >EmptyPoint.X	) && (bx.Location.Y	== EmptyPoint.Y	 )))
						{
							switch (d)
							{
								case 0 : Functions.MoveRight(bx);
									break;
								case 2:	Functions.MoveLeft(bx);
									break;
									
							}

						}
					}
				}
				
			EmptyPoint = tempEmptyPoint;	
		}
		}

		/// <summary>
		/// Find out the direction in which we have to move the buttons
		/// </summary>
		/// <param name="Loc">The Location of the Button Clicked</param>
		/// <param name="ep">The EmptyPoint Co-ordinates</param>
		/// <returns></returns>
		private int Direction( Point Loc,Point ep)
		{
			int valuetoreturn=0;
			if ((Loc.X < ep.X) && (Loc.Y == ep.Y ))
				valuetoreturn = 0;
			else if ((Loc.X > ep.X) && (Loc.Y == ep.Y ))
				valuetoreturn= 2;
			else if ((Loc.Y < ep.Y) && (Loc.X == ep.X))
				valuetoreturn = 3;
			else if ((Loc.Y > ep.Y )&&( Loc.X == ep.X))
				valuetoreturn= 1;
			return valuetoreturn;
		}

		//This function Randomizes the Buttons
		private void Randomize ()
		{
			Random r = new Random ();
			Button tempBtn = new Button();
			for (int i =0; i < 100; i++)
			{
				// Choose a Random Button from all the buttons present
				tempBtn = (Button) alAllButtons[r.Next(alAllButtons.Count )];

				// Move it
				MoveButton(tempBtn);				
			} // Repeat this loop 100 times.

		}

		// If a button is clicked move it towards the Empty Space
		private void OnButtonClick(object sender, System.EventArgs e)
		{		
			MoveButton((Button)sender);
		}

		// This function takes an Bitmap Image, crops it from the points passed as
		// parameter and returns back a cropped image
		// But this function is not useful Right now.
		// It'll help when i'll upgrade the project
		public Image CropAndReturn(Bitmap ToBeCropped, Bitmap NewBitmap,int x, int y)
		{
			Bitmap b = new Bitmap(x,y);
			for(int i =0; i< x; i++)
				for(int j = 0; j< y; j++)
					b.SetPixel (i,j,ToBeCropped.GetPixel (i,j));
			return b;
			
		}

		/// <summary>
		/// This function takes a Image and makes pieces 15 pieces of it, of 80x80 pixels
		/// Exactly of the size of a button
		/// </summary>
		/// <param name="ToBeCropped"></param>
		/// <param name="x">The width of the small images</param>
		/// <param name="y">The Height of the small images</param>
		/// <returns></returns>
		public ArrayList ReturnCroppedList(Bitmap ToBeCropped,int x,int y)
		{
			ArrayList ilTemp = new ArrayList ();
			int h,v;
			h=v=0;
			for(int k = 0; k <15; k++)
			{
				Bitmap b = new Bitmap(x,y);

				for(int i =0; i< x; i++)
					for(int j = 0; j< y; j++)
						b.SetPixel (i,j,ToBeCropped.GetPixel ((i+h),(j+v)));
				ilTemp.Add(b);
				
				h+=80;
				if (h == 320)
				{h=0; v+=80;}
				
			}
			return ilTemp;
		}

		/// <summary>
		/// This function binds the Images to the button
		/// </summary>
		public void AddImagesToButtons()
		{
			ilSmallImages = ReturnCroppedList(MainBitmap,80,80);
			button1.Image  = (Image)ilSmallImages[0];
			button2.Image = (Image)ilSmallImages[1];
			button3.Image = (Image)ilSmallImages[2];
			button4.Image = (Image)ilSmallImages[3];
			button5.Image = (Image)ilSmallImages[4];
			button6.Image = (Image)ilSmallImages[5];
			button7.Image = (Image)ilSmallImages[6];
			button8.Image = (Image)ilSmallImages[7];
			button9.Image = (Image)ilSmallImages[8];
			button10.Image = (Image)ilSmallImages[9];
			button11.Image = (Image)ilSmallImages[10];
			button12.Image = (Image)ilSmallImages[11];
			button13.Image = (Image)ilSmallImages[12];
			button14.Image = (Image)ilSmallImages[13];
			button15.Image = (Image)ilSmallImages[14];
		
		}

		
		/// <summary>
		/// This function gets a image from the database
		/// </summary>
		/// <param name="c">This character specifies whether to get a Normal Image
		/// or the "Expert Mode" Image</param>
		/// <returns></returns>
		private Bitmap GetBitmapFromDatabase(char c)
		{

			Bitmap b = new Bitmap (320,320);
			Random r = new Random ();
			string sql = "Select * from Bitmaps";
			string strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source="+Application.StartupPath +"\\game.mdb";
			OleDbConnection Conn = new OleDbConnection (strConn);
			try 
			{
				Conn.Open();
				System.Data.OleDb.OleDbDataAdapter da = new OleDbDataAdapter (sql,Conn);
				DataSet ds = new DataSet ();
				da.Fill (ds,"Bitmaps");
				DataTable dt = ds.Tables["Bitmaps"];
				DataRow dr;
				do
				{
					dr= dt.Rows [r.Next (dt.Rows.Count )];
					b=(Bitmap) Bitmap.FromFile (Application.StartupPath + "\\Images\\"+dr["Picture"].ToString ());
					
				}while(dr["Type"].ToString () != c.ToString ());
			
			}
			catch (Exception ex)
			{
				MessageBox.Show (ex.Message.ToString ());
				Conn.Close ();
			}
			finally
			{
				Conn.Close ();
			}
			return b;
		}

		
		void NewGame()
		{			
			ilSmallImages = new ArrayList ();    
			// This function gets the Main picture, shreds it and binds the small
			// images to the button
			AddImagesToButtons();

			// This function randomizes the buttons
			Randomize();

		}

		// Randomize th images
		private void OnButtonReStartClick(object sender, System.EventArgs e)
		{
			Randomize();
		}

		private void OnmiFileNewGameClick(object sender, System.EventArgs e)
		{
			NewGame();
		}

		private void buttonRefresh_Click(object sender, System.EventArgs e)
		{
			Graphics g = this.CreateGraphics ();
			g.DrawImage (MainBitmap,375,10,MainBitmap.Width ,MainBitmap.Height);
						
			g.DrawString ("Made by Tushar Agarwal",this.Font ,Brushes.MistyRose ,375,20);   
			g.Dispose ();
		}

		private void OnbuttonChangeImageClick(object sender, System.EventArgs e)
		{
			MainBitmap = GetBitmapFromDatabase('N');
			NewGame();
		}

		private void OnmiFileModeNumPic(object sender, System.EventArgs e)
		{
			miFileModeNumber.Checked = false;
			miFileModePicture.Checked = false;
			miFileModeNumPic.Checked = true;
			ShowNumbersOnButtons();
		}

		// This functin shows the numbers on the button
		void ShowNumbersOnButtons()
		{
			button1.Text = "1";
			button2.Text = "2";
			button3.Text = "3";
			button4.Text = "4";
			button5.Text = "5";
			button6.Text = "6";
			button7.Text = "7";
			button8.Text = "8";
			button9.Text = "9";
			button10.Text = "10";
			button11.Text = "11";
			button12.Text = "12";
			button13.Text = "13";
			button14.Text = "14";
			button15.Text = "15";
		}
		// This function erases the numbers written on the button
		void HideNumbersFromButtons()
		{
			button1.Text = "";
			button2.Text = "";
			button3.Text = "";
			button4.Text = "";
			button5.Text = "";
			button6.Text = "";
			button7.Text = "";
			button8.Text = "";
			button9.Text = "";
			button10.Text = "";
			button11.Text = "";
			button12.Text = "";
			button13.Text = "";
			button14.Text = "";
			button15.Text = "";
		}

		private void OnmiFileModePictureClick(object sender, System.EventArgs e)
		{
		
			miFileModeNumber.Checked = false;
			miFileModePicture.Checked = true;
			miFileModeNumPic.Checked = false;
			HideNumbersFromButtons();
		}

		
		private void OnmiFileChangePicture(object sender, System.EventArgs e)
		{
			MainBitmap = GetBitmapFromDatabase('N');
			NewGame();
		}

		private void OnmiFileNumberModeClick(object sender, System.EventArgs e)
		{
			// This have to be still made
			MessageBox.Show ("This function hasn't implemented yet");
		}

		private void OnFileExitClick(object sender, System.EventArgs e)
		{
			Application.Exit ();
		}

		private void OnmiHelpAboutClick(object sender, System.EventArgs e)
		{
			string temp;
			temp = "Made by    : TUSHAR AGARWAL" +
				 "\nEMail         : tushar_ag@yahoo.com" +
				 "\nPhone         : 98680 52445";
			MessageBox.Show (temp,"About Sliding Puzzle");
		}
		
             
	}
}
