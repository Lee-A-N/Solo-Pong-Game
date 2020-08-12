Solo Pong is a small arcade game for the Meadow with Hack Kit Pro.
- It uses a rotary encoder for paddle movement.  Press the encoder knob for game start/restart.
- It uses the ST7789 LCD display
- A 3-position On/Off/On SPDT switch is used for setting the volume level (Mute, Soft, or Normal)
- The piezo speaker is used for sound effects

One point is scored each time the ball hits the paddle.

The game gets more difficult as it progresses.  The paddle shrinks each time the ball hits the
paddle until a minimum size is reached.  Subtle changes are made to the direction of the ball
following each surface hit. For paddle hits, the angle change is different depending on where
the ball strikes the paddle.

Enjoy!


Hardware: Meadow F7 board, Rotary Encoder, 3-position SPDT On/Off/On switch, ST7789 LDC display, 
             Piezo speaker, 0.1 microFarad capacitor, 2 10K resistors

All of the hardware is include in the Pro Hack Kit.

Wiring:

 SPDT 3-position On/Off/On switch (used for volume settings Mute/Soft/Normal):
     top - 10K resistor to ground, pin D03
     middle - Vcc
     bottom - 10K resistor to ground, pin D04

 Rotary Encoder:
     CLK - pin D10
     DT - pin D09
     SW - 0.1 microFarad capacitor to ground, pin D08
     + - Vcc
     GND - Ground

 Piezo Speaker:
     Red: Pin D07
     Black: Ground

 ST7789 Display:
     GND: Ground
     VCC: Vcc
     SCL: SCK
     SDA: MOSI
     RES: Pin D00
     DC: Pin D01
     
 ------------------------------------------
 NuGet Packages needed:
     Meadow.Foundation
     Meadow.Foundation.Displays.TftSpi
