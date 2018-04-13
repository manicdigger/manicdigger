public class TextPart
{
	internal int color;
	internal string text;
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
					if (ColorCi.ExtractA(c) > 0)
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
		int wordMax = 64;

		// Prepare temporary arrays
		TextPart[] parts = new TextPart[messageMax];
		int partsCount = 0;
		int[] currenttext = new int[wordMax];
		int currenttextLength = 0;
		bool endCurrentWord = false;

		// Split the given string into single characters
		IntRef sLength = new IntRef();
		int[] sChars = platform.StringToCharArray(s, sLength);

		// Set default color
		int currentColor = defaultcolor;
		bool changeColor = false;
		int nextColor = defaultcolor;

		// Process each character
		for (int i = 0; i < sLength.value; i++)
		{
			if (partsCount >= messageMax)
			{
				// Quit parsing text if message has reached maximum length
				break;
			}

			if (endCurrentWord || currenttextLength >= wordMax)
			{
				if (currenttextLength > 0)
				{
					//Add content so far to return value
					TextPart part = new TextPart();
					part.text = platform.CharArrayToString(currenttext, currenttextLength);
					part.color = currentColor;
					parts[partsCount] = part;
					partsCount++;
					currenttextLength = 0;
				}
				endCurrentWord = false;
			}

			if (changeColor)
			{
				currentColor = nextColor;
				changeColor = false;
			}

			if (sChars[i] == ' ')
			{
				// Begin a new word if a space character is found
				currenttext[currenttextLength] = sChars[i];
				currenttextLength++;
				endCurrentWord = true;
			}
			else if (sChars[i] == '&')
			{
				// If a & is found, try to parse a color code
				if (i + 1 < sLength.value)
				{
					int color = HexToInt(sChars[i + 1]);
					if (color != -1)
					{
						// Update current color and end word
						nextColor = GetColor(color);
						changeColor = true;
						endCurrentWord = true;

						// Increment i to prevent the code from being read again
						i++;

						continue;
					}
					else
					{
						// No valid color code found. Display as normal character
						currenttext[currenttextLength] = sChars[i];
						currenttextLength++;
					}
				}
				else
				{
					// End of string. Display as normal character
					currenttext[currenttextLength] = sChars[i];
					currenttextLength++;
				}
			}
			else
			{
				// Nothing special. Just add the current character
				currenttext[currenttextLength] = sChars[i];
				currenttextLength++;
			}
		}

		// Add any leftover text parts in current color
		if (currenttextLength != 0 && partsCount < messageMax)
		{
			TextPart part = new TextPart();
			part.text = platform.CharArrayToString(currenttext, currenttextLength);
			part.color = currentColor;
			parts[partsCount] = part;
			partsCount++;
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
			case 0: { return ColorCi.FromArgb(255, 0, 0, 0); }
			case 1: { return ColorCi.FromArgb(255, 0, 0, 191); }
			case 2: { return ColorCi.FromArgb(255, 0, 191, 0); }
			case 3: { return ColorCi.FromArgb(255, 0, 191, 191); }
			case 4: { return ColorCi.FromArgb(255, 191, 0, 0); }
			case 5: { return ColorCi.FromArgb(255, 191, 0, 191); }
			case 6: { return ColorCi.FromArgb(255, 191, 191, 0); }
			case 7: { return ColorCi.FromArgb(255, 191, 191, 191); }
			case 8: { return ColorCi.FromArgb(255, 40, 40, 40); }
			case 9: { return ColorCi.FromArgb(255, 64, 64, 255); }
			case 10: { return ColorCi.FromArgb(255, 64, 255, 64); }
			case 11: { return ColorCi.FromArgb(255, 64, 255, 255); }
			case 12: { return ColorCi.FromArgb(255, 255, 64, 64); }
			case 13: { return ColorCi.FromArgb(255, 255, 64, 255); }
			case 14: { return ColorCi.FromArgb(255, 255, 255, 64); }
			case 15: { return ColorCi.FromArgb(255, 255, 255, 255); }
			default: return ColorCi.FromArgb(255, 255, 255, 255);
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
