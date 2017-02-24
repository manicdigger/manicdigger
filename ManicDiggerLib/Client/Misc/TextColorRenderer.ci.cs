public class TextPart
{
	internal int color;
	internal string text;
	internal bool spaceAfter;
}

/// <summary>
/// Contains various methods for text rendering
/// </summary>
public class TextColorRenderer
{
	internal GamePlatform platform;

	/// <summary>
	/// Creates a bitmap from a given string
	/// </summary>
	/// <param name="t">The <see cref="Text_"/> object to create an image from</param>
	/// <returns>A <see cref="BitmapCi"/> containing the rendered text</returns>
	internal BitmapCi CreateTextTexture(Text_ t)
	{
		IntRef partsCount = new IntRef();
		TextPart[] parts = DecodeColors(t.text, t.color, partsCount);

		float totalwidth = 0;
		float totalheight = 0;
		int[] sizesX = new int[partsCount.value];
		int[] sizesY = new int[partsCount.value];

		for (int i = 0; i < partsCount.value; i++)
		{
			IntRef outWidth = new IntRef();
			IntRef outHeight = new IntRef();
			platform.TextSize(parts[i].text, t.font, outWidth, outHeight);

			sizesX[i] = outWidth.value;
			sizesY[i] = outHeight.value;

			totalwidth += outWidth.value;
			totalheight = MathCi.MaxFloat(totalheight, outHeight.value);
		}

		int size2X = NextPowerOfTwo(platform.FloatToInt(totalwidth) + 1);
		int size2Y = NextPowerOfTwo(platform.FloatToInt(totalheight) + 1);
		BitmapCi bmp2 = platform.BitmapCreate(size2X, size2Y);
		int[] bmp2Pixels = new int[size2X * size2Y];

		float currentwidth = 0;
		for (int i = 0; i < partsCount.value; i++)
		{
			int sizeiX = sizesX[i];
			int sizeiY = sizesY[i];
			if (sizeiX == 0 || sizeiY == 0)
			{
				continue;
			}
			
			Text_ partText = new Text_();
			partText.text = parts[i].text;
			partText.color = parts[i].color;
			partText.font = t.font;
			
			BitmapCi partBmp = platform.CreateTextTexture(partText);
			int partWidth = platform.FloatToInt(platform.BitmapGetWidth(partBmp));
			int partHeight = platform.FloatToInt(platform.BitmapGetHeight(partBmp));
			int[] partBmpPixels = new int[partWidth * partHeight];
			platform.BitmapGetPixelsArgb(partBmp, partBmpPixels);
			for (int x = 0; x < partWidth; x++)
			{
				for (int y = 0; y < partHeight; y++)
				{
					if (x + currentwidth >= size2X) { continue; }
					if (y >= size2Y) { continue; }
					int c = partBmpPixels[MapUtilCi.Index2d(x, y, partWidth)];
					if (Game.ColorA(c) > 0)
					{
						bmp2Pixels[MapUtilCi.Index2d(platform.FloatToInt(currentwidth) + x, y, size2X)] = c;
					}
				}
			}
			currentwidth += sizeiX;
		}
		platform.BitmapSetPixelsArgb(bmp2, bmp2Pixels);
		return bmp2;
	}

	/// <summary>
	/// Split a given string into words with different colors
	/// </summary>
	/// <param name="s">String to split</param>
	/// <param name="defaultcolor">Default color to use when no color code is given</param>
	/// <param name="retLength"><see cref="IntRef"/> the number of text parts will be written to</param>
	/// <returns><see cref="TextPart"/> array containing the processed parts of the given string</returns>
	public TextPart[] DecodeColors(string s, int defaultcolor, IntRef retLength)
	{
		// Maximum word/message length
		int messageMax = 256;
		int wordMax = 256;
		
		// Prepare temporary arrays
		TextPart[] parts = new TextPart[messageMax];
		int partsCount = 0;
		int[] currenttext = new int[wordMax];
		int currenttextLength = 0;
		
		// Split the given string into single characters
		IntRef sLength = new IntRef();
		int[] sChars = platform.StringToCharArray(s, sLength);
		
		// Set default color
		int currentcolor = defaultcolor;
		
		// Process each character
		for (int i = 0; i < sLength.value; i++)
		{
			// If a & is found, try to parse a color code
			if (sChars[i] == '&')
			{
				//check if there's a character after it
				if (i + 1 < sLength.value)
				{
					//try to parse the color code
					int color = HexToInt(sChars[i + 1]);
					if (color != -1)
					{
						//Color has been parsed successfully
						if (currenttextLength != 0)
						{
							//Add content so far to return value
							TextPart part = new TextPart();
							part.text = platform.CharArrayToString(currenttext, currenttextLength);
							part.color = currentcolor;
							if (partsCount >= messageMax)
							{
								// Quit parsing text is message has reached maximum length
								break;
							}
							parts[partsCount] = part;
							partsCount++;
						}
						//Update current color and reset current text length
						currenttextLength = 0;
						currentcolor = GetColor(color);

						//Increment i to prevent the code from being read again
						i++;
					}
					else
					{
						//no valid color code found. display as normal character
						if (currenttextLength >= wordMax)
						{
							// Skip all input exceeding maximum word length
							continue;
						}
						currenttext[currenttextLength] = sChars[i];
						currenttextLength++;
					}
				}
				else
				{
					//if not, just display it as normal character
					if (currenttextLength >= wordMax)
					{
						// Skip all input exceeding maximum word length
						continue;
					}
					currenttext[currenttextLength] = sChars[i];
					currenttextLength++;
				}
			}
			else
			{
				// If a space character is found begin a new word
				if (platform.IsFastSystem() && sChars[i] == ' ')
				{
					// Word boundary detected
					if (currenttextLength != 0)
					{
						string word = platform.CharArrayToString(currenttext, currenttextLength);
						if (platform.StringEmpty(word))
						{
							currenttextLength = 0;
							continue;
						}
						//Add content so far to return value
						TextPart part = new TextPart();
						part.text = word;
						part.color = currentcolor;
						// Specify that some space should be left between this and the next word
						part.spaceAfter = true;
						if (partsCount >= messageMax)
						{
							// Quit parsing text is message has reached maximum length
							break;
						}
						parts[partsCount] = part;
						partsCount++;
					}
					// Reset length counter
					currenttextLength = 0;
				}
				else
				{
					//Nothing special. Just add the current character
					if (currenttextLength >= wordMax)
					{
						// Skip all input exceeding maximum word length
						continue;
					}
					currenttext[currenttextLength] = sChars[i];
					currenttextLength++;
				}
			}
		}
		
		//Add any leftover text parts in current color
		if (currenttextLength != 0)
		{
			TextPart part = new TextPart();
			part.text = platform.CharArrayToString(currenttext, currenttextLength);
			part.color = currentcolor;
			if (partsCount < messageMax)
			{
				// Only add text if message is not full yet
				parts[partsCount] = part;
				partsCount++;
			}
		}
		
		// Set length of returned array and return result
		retLength.value = partsCount;
		return parts;
	}

	int NextPowerOfTwo(int x)
	{
		x--;
		x |= x >> 1;  // handle  2 bit numbers
		x |= x >> 2;  // handle  4 bit numbers
		x |= x >> 4;  // handle  8 bit numbers
		x |= x >> 8;  // handle 16 bit numbers
		//x |= x >> 16; // handle 32 bit numbers
		x++;
		return x;
	}

	/// <summary>
	/// Fetches the corresponding ARGB color value for a hexadecimal color code
	/// </summary>
	/// <param name="currentcolor">Color code value given as decimal number</param>
	/// <returns>An ARGB color value</returns>
	static int GetColor(int currentcolor)
	{
		switch (currentcolor)
		{
				case 0: { return Game.ColorFromArgb(255, 0, 0, 0); }
				case 1: { return Game.ColorFromArgb(255, 0, 0, 191); }
				case 2: { return Game.ColorFromArgb(255, 0, 191, 0); }
				case 3: { return Game.ColorFromArgb(255, 0, 191, 191); }
				case 4: { return Game.ColorFromArgb(255, 191, 0, 0); }
				case 5: { return Game.ColorFromArgb(255, 191, 0, 191); }
				case 6: { return Game.ColorFromArgb(255, 191, 191, 0); }
				case 7: { return Game.ColorFromArgb(255, 191, 191, 191); }
				case 8: { return Game.ColorFromArgb(255, 40, 40, 40); }
				case 9: { return Game.ColorFromArgb(255, 64, 64, 255); }
				case 10: { return Game.ColorFromArgb(255, 64, 255, 64); }
				case 11: { return Game.ColorFromArgb(255, 64, 255, 255); }
				case 12: { return Game.ColorFromArgb(255, 255, 64, 64); }
				case 13: { return Game.ColorFromArgb(255, 255, 64, 255); }
				case 14: { return Game.ColorFromArgb(255, 255, 255, 64); }
				case 15: { return Game.ColorFromArgb(255, 255, 255, 255); }
				default: return Game.ColorFromArgb(255, 255, 255, 255);
		}
	}

	/// <summary>
	/// Converts a single hexadecimal character to its decimal value
	/// </summary>
	/// <param name="c">Hexadecimal character to convert</param>
	/// <returns>The decimal value of the given character if valid. -1 otherwise.</returns>
	static int HexToInt(int c)
	{
		if (c == '0') { return 0; }
		if (c == '1') { return 1; }
		if (c == '2') { return 2; }
		if (c == '3') { return 3; }
		if (c == '4') { return 4; }
		if (c == '5') { return 5; }
		if (c == '6') { return 6; }
		if (c == '7') { return 7; }
		if (c == '8') { return 8; }
		if (c == '9') { return 9; }
		if (c == 'a') { return 10; }
		if (c == 'b') { return 11; }
		if (c == 'c') { return 12; }
		if (c == 'd') { return 13; }
		if (c == 'e') { return 14; }
		if (c == 'f') { return 15; }
		return -1;
	}
}
