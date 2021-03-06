﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tetris;
using System.Media;

namespace Tetris
{
    public partial class TetrisGame : Form
    {
        /// <summary>
        /// A game of Tetris implemented as a WinForms application
        /// </summary>
        public TetrisGame()
        {
            InitializeComponent();
			createSquares(squares);
			createSquares(pauseSquares);
            blocksetList.DataSource = BlockLoader.names();
        }

        #region variables

        /// <summary>
        /// Is a game currently being played?
        /// </summary>
        Boolean playing { get; set; }

        /// <summary>
        /// The board that the game is played on
        /// </summary>
        Board board;

        /// <summary>
        /// The number of visible columns on the board
        /// </summary>
        int numberOfColumns = 10;

        /// <summary>
        /// The number of visible rows on the board
        /// </summary>
        int numberOfRows = 20;

        /// <summary>
        /// The size in px of each side of the squares
        /// </summary>
        int squareDimensions = 20;

        /// <summary>
        /// The squares that are visible on the board
        /// </summary>
        Dictionary<string, Square> squares = new Dictionary<string,Square>();

		/// <summary>
		/// The squares that are for pause game on the board
		/// </summary>
		Dictionary<string, Square> pauseSquares = new Dictionary<string,Square>();

        /// <summary>
        /// The keybord input for the game
        /// </summary>
        Input input = new Input();

        #endregion variables

        #region game

        /// <summary>
        /// Resets the game
        /// </summary>
        private void resetGame()
        {
            squareDimensions = (int)sqSizeSelect.Value;
            numberOfRows = (int)boardRowsSelect.Value;
            numberOfColumns = (int)boardColsSelect.Value;
            board = new Board(numberOfRows, numberOfColumns, blocksetList.Text);
			createSquares(squares);
			createSquares(pauseSquares);
            tickTimer.Enabled = true;
            playing = true;

        }

        /// <summary>
        /// Update the game at the interval that is specified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tickTimer_Tick(object sender, EventArgs e)
        {
            if (playing)
            {

				if (board.currentBlock == null || !board.canDropFurther ()) {
					board.spawnBlock ();
					if (!board.canBeHere (board.currentBlock)) {
						resetGame ();
					}

				}
                board.tick();
                updateBoard();
                rowsCleared.Text = board.rowsDestroyed.ToString();
                score.Text = board.score.ToString();
            }
        }

        #endregion game

        #region GUI

        /// <summary>
        /// Calculates a string to use as the key for the squares hash
        /// </summary>
        private String squaresKey(int row, int col)
        {
            return "R" + row.ToString() + "C" + col.ToString();
        }

        /// <summary>
        /// Creates the squares which make up the visible portion of the board
        /// </summary>
		private void createSquares(Dictionary<string, Square> sqr)
        {
			foreach (KeyValuePair<String,Square> val in sqr)
            {
                val.Value.Dispose();
            }
			sqr.Clear();

            for (int row = 0; row < numberOfRows; row++)
            {
                for (int col = 0; col < numberOfColumns; col++)
                {
                    Square square = new Square(row, col);
                    square.Width = squareDimensions;
                    square.Height = squareDimensions;
                    square.Parent = gameWindow;
                    square.Top = row * squareDimensions;
                    square.Left = col * squareDimensions;

					sqr.Add(squaresKey(row, col), square);
                }
            }
        }

        /// <summary>
        /// Updates the game board, displaying where the squares are on the grid
        /// </summary>
        private void updateBoard()
        {
            // update the color of each of the squares on the board
            Square square;
            for (int row = 0; row < numberOfRows; row++)
            {
                for (int col = 0; col < numberOfColumns; col++)
                {
                    squares.TryGetValue(squaresKey(row, col), out square);
                    square.color = board.board[col, row + board.hiddenRows];
                }
            }

            // then display the current block
            Block block = board.currentBlock; // cache the block to make the code read a bit better

            int range = board.howFar (block);
            if (range >= 2)
            {
                // Display ghost block
                for (int row = 0; row < block.squares.GetLength(0); row++)
                {
                    for (int col = 0; col < block.squares.GetLength(1); col++)
                    {
                        Coordinate coord = new Coordinate(col, row);
                        coord = block.toBoardCoordinates(coord);
                        if (block.squares[row, col] && coord.x >= 0 && coord.x < numberOfColumns
                                && coord.y + range >= board.hiddenRows && coord.y + range < numberOfRows + board.hiddenRows)
                        {
                            squares.TryGetValue(squaresKey(coord.y - board.hiddenRows + range, coord.x), out square);
                            square.color = Color.LightGray.ToArgb();
                        }
                    }
                }
            }

            for (int row = 0; row < block.squares.GetLength(0); row++)
            {
                for (int col = 0; col < block.squares.GetLength(1); col++)
                {
                    Coordinate coord = new Coordinate(col, row);
                    coord = block.toBoardCoordinates(coord);
                    if (block.squares[row, col] && coord.x >= 0 && coord.x < numberOfColumns
                            && coord.y >= board.hiddenRows && coord.y < numberOfRows + board.hiddenRows)
                    {
                        squares.TryGetValue(squaresKey(coord.y - board.hiddenRows, coord.x), out square);
                        square.color = block.color.ToArgb();
                    }
                }
            }
        }

		private void updatepauseboard(){
			Square pauseSquare;
			for (int row = 0; row < numberOfRows; row++)
			{
				for (int col = 0; col < numberOfColumns; col++)
				{
					pauseSquares.TryGetValue(squaresKey(row, col), out pauseSquare);
					pauseSquare.color = Color.PeachPuff.ToArgb();
					if (col != numberOfColumns) {
						col++;
						pauseSquares.TryGetValue(squaresKey(row, col), out pauseSquare);
						pauseSquare.color = board.currentBlock.color.ToArgb();	
					}
				}
			}
		}

        #endregion GUI

        #region input

        /// <summary>
        /// Create a new game when the 'New Game' button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newGameButton_Click(object sender, EventArgs e)
        {
            resetGame();
        }

        /// <summary>
        /// Listen for input from the keyboard and respond accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TetrisGame_KeyPress(object sender, KeyPressEventArgs e)
        {
            textBox1.Text = e.KeyChar.ToString();
            if (playing)
			{
				if (!board.ispause) {
					if (input.downKeyPressed) {
						board.spdMinus = 2;
						board.lowerBlock ();
					}
					if(input.speedKeyPressed){
						board.downPress = 1;
						board.spdMinus = 2;
						board.lowerBlock ();
					}
					if (input.leftKeyPressed) {
						board.moveBlockLeft ();
					}
					if (input.rightKeyPressed) {
						board.moveBlockRight ();
					}
					if (input.rotateKeyPressed) {
						board.rotateBlock ();
					}
					if (input.swapKeyPressed) {
						board.swapBlock ();
					}
				}
				if (input.pauseKeyPressed) {
					if (board.ispause) {
						board.ispause = false;		
						Dictionary<string, Square> sqr = new Dictionary<string,Square>();
						sqr = squares;
						squares = pauseSquares;
						pauseSquares = sqr;
					}else{
						board.ispause = true;
						Dictionary<string, Square> sqr = new Dictionary<string,Square>();
						sqr = squares;
						squares = pauseSquares;
						pauseSquares = sqr;
						updatepauseboard ();
					}
				}
                updateBoard();
            }
        }

        /// <summary>
        /// When a key is pressed, let the input controller process the action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TetrisGame_KeyDown(object sender, KeyEventArgs e)
        {
            textBox2.Text = e.KeyValue.ToString() + " " + e.KeyCode.ToString();
            char key = e.KeyCode.ToString().ToLower()[0];
            input.processKey(key, true);
        }

        /// <summary>
        /// When a key is raised, let the input controller process the action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TetrisGame_KeyUp(object sender, KeyEventArgs e)
        {
            char key = e.KeyCode.ToString().ToLower()[0];
            input.processKey(key, false);
        }

        #endregion input

    }
}
