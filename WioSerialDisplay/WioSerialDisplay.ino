#include "Free_Fonts.h" // Include the header file attached to this sketch

#include "SPI.h"
#include "TFT_eSPI.h"

// Use hardware SPI
TFT_eSPI tft = TFT_eSPI();
TFT_eSprite screen = TFT_eSprite(&tft);

#define BACKGROUND_COLOR TFT_BLACK

void setup(void)
{
    tft.begin();
    tft.setRotation(3);

    screen.createSprite(tft.width(), tft.height());

    screen.fillScreen(BACKGROUND_COLOR);

    screen.pushSprite(0, 0);
}

void loop()
{
  handleSerialCommand();
}

void handleSerialCommand()
{
  char *line = readLine();

  if (line != 0)
  {
    Serial.println(line);
    
    char *word = strtok(line, " ");

    if (strcmp(word, "Reset") == 0)
    {     
      screen.fillRect(0, 0, tft.width(), tft.height(), BACKGROUND_COLOR);
    }
    else if (strcmp(word, "Set") == 0)
    {
      word = strtok(NULL, " ");
      
      if (strcmp(word, "Preset") == 0)
      {
        char *presetName = strtok(NULL, " ");

        char *textColor = strtok(NULL, "");
        
        setPresetName(presetName, (textColor != NULL) ? (uint)strtol(textColor, 0, 16) : TFT_WHITE);
      }    
      else if (strcmp(word, "Stomp") == 0)
      {
        word = strtok(NULL, " ");

        if (word)
        {
          int stompNumber = word[0] - '1';

          if ((stompNumber >= 0) && (stompNumber < 3))
          {
            char *text1 = strtok(NULL, " ");

            if (!text1)
            {
              setStomp(stompNumber, NULL, NULL, 0, 0);
            }
            else
            {           
              char *text2 = strtok(NULL, " ");
  
              char *bgColor = strtok(NULL, " ");
              char *fgColor = strtok(NULL, "");
              
              if (bgColor && fgColor)
              {                
                setStomp(stompNumber, text1, text2, (uint)strtol(bgColor, 0, 16), (uint)strtol(fgColor, 0, 16));
              }
            }
          }
        }
      }
      else if (strcmp(word, "Tuner") == 0)
      {
        char *noteName = strtok(NULL, " ");

        if (noteName != NULL)
        {
          setTuner(noteName, atoi(strtok(NULL, "")));
        }
        else
        {
          setTuner("\0", -1000);
        }
      }
    }
       
    screen.pushSprite(0, 0);
  }
}

const int numChars = 256;
char receivedChars[numChars];

char *readLine()
{
  static byte ndx = 0;
  char rc;
 
  while (Serial.available() > 0)
  {
    rc = Serial.read();
    
    if (rc != '\n')
    {
      receivedChars[ndx] = rc;      
      ndx++;

      if (ndx >= numChars)
      {
        ndx = numChars - 1;
      }
    }
    else
    {    
      receivedChars[ndx] = '\0'; // terminate the string
      ndx = 0;

      return receivedChars;
    }
  }

  return 0;
}

int patchBoxStartY = 0;
int patchBoxHeight = 50;

void setPresetName(char *presetName, uint textColor)
{
    screen.fillRect(0, patchBoxStartY, 320, patchBoxHeight, BACKGROUND_COLOR);
  
    screen.setTextColor(textColor);
    screen.setTextDatum(MC_DATUM);
 
    screen.setFreeFont(FSSB18);
    screen.drawString(presetName, 160, patchBoxStartY + (patchBoxHeight / 2), GFXFF);
}

int stompBoxHeight = 70;
int stompBoxStartY = 240 - stompBoxHeight;

void setStomp(int stompNumber, char *text1, char *text2, uint bgColor, uint fgColor)
{
    int stompWidth = screen.width() / 3;
  
    if (!text1)
    {
      screen.fillRect((320 / 3) * stompNumber, stompBoxStartY, 320 / 3, stompBoxHeight, BACKGROUND_COLOR);   

      return;
    }
      
    screen.fillRect(stompWidth * stompNumber, stompBoxStartY, stompWidth, stompBoxHeight, bgColor);    
    screen.drawRect(stompWidth * stompNumber, stompBoxStartY, stompWidth, stompBoxHeight, fgColor);
  
    screen.setTextColor(fgColor);
    screen.setTextDatum(MC_DATUM);

    int xCenter = stompWidth * (stompNumber + 0.5);

    screen.setFreeFont(FSSB12);

    int textWidth = max(screen.textWidth(text1), screen.textWidth(text2));

    if(textWidth > (stompWidth - 4))
    {
      screen.setFreeFont(FSSB9);
    }
    
    screen.drawString(text1, xCenter, stompBoxStartY + (stompBoxHeight / 4), GFXFF);
    screen.drawString(text2, xCenter, stompBoxStartY + (stompBoxHeight * .75), GFXFF);
}

char lastNote = 0;
int lastDelta = -1000;

void setTuner(char *noteName, int delta)
{
    if ((lastNote == *noteName) && (lastDelta == delta))
      return;

    lastNote = *noteName;
    lastDelta = delta;
    
    int xCenter = screen.width() / 2;
    int yCenter = screen.height() / 2;

    int tunerHeight = 30;
    int centerWidth = 35;
    int deltaWidth = 10;
    
    screen.fillRect(0, yCenter - tunerHeight, tft.width(), tunerHeight * 2, BACKGROUND_COLOR);

    if (*noteName == 0)
      return;

    if ((delta >= -1) && (delta <= 1))
    {
      screen.fillRect(xCenter - centerWidth, yCenter - tunerHeight, centerWidth * 2, tunerHeight * 2, TFT_BLUE);    
    }
    
    if (delta < 0)
    {
      for (int i = 0; i > delta; i--)
      {
        screen.fillRect(xCenter - centerWidth + (deltaWidth * (i - 1) * 2),
          yCenter - tunerHeight, (deltaWidth - 2) * 2, tunerHeight * 2, TFT_RED);           
      }
    }
    else if (delta > 0)
    {
      for (int i = 0; i < delta; i++)
      {
        screen.fillRect(xCenter + centerWidth + (deltaWidth * i * 2),
          yCenter - tunerHeight, (deltaWidth - 2) * 2, tunerHeight * 2, TFT_YELLOW);           
      }      
    }
    
    screen.setTextColor(TFT_WHITE);
    screen.setTextDatum(MC_DATUM);

    screen.setFreeFont(FSSB24);

    screen.drawString(noteName, xCenter, yCenter, GFXFF);
}
