using System;
using System.Windows .Forms ;
using System.Drawing ;

namespace Tushar.SlidingPuzzle
{
	/// <summary>
	/// Summary description for Functions.
	/// </summary>
	public class Functions
	{
		public Functions()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public static void  MoveLeft(Button b)
		{
			b.Location =  new Point(b.Location.X - b.Size.Width ,b.Location.Y );
		}
		public static void  MoveRight (Button b)
		{
			b.Location = new Point(b.Location.X + b.Size.Width ,b.Location.Y  );
		}
		public static void MoveUp(Button b)
		{
			b.Location = new Point(b.Location .X,b.Location.Y  - b.Size.Height );
		}
		public static void MoveDown(Button b)
		{
			b.Location = new Point(b.Location .X,b.Location.Y  + b.Size.Height );
		}




	}
}
