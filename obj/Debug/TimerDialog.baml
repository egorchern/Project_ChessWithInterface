   M S B A M L   `   `   `  ����U  QProject_ChessWithInterface, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:Project_ChessWithInterfaceProject_ChessWithInterface  5   	   ,    &Project_ChessWithInterface.TimerDialog   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   :local(clr-namespace:Project_ChessWithInterface     5   	   -   5          �Name$  TimersWindow��5   	    � Title$ TimerDialog��5   	   $��290.279��6   $	��757��6.    � 
ResizeMode$ NoResize=�6:    � WindowStartupLocation$ CenterScreen=�6P   .��5	      � .+�5
   
   �� -   5         ��Name$ WhiteTime_txt��5
      $
��Left=�6)   $��23��6D   $��
89,121,0,0q�6P    �� TextWrapping$
 Wrap=�6d   $
��5:05��6x   $	��Top=�6�   $	��120��6�    J� RenderTransformOrigin$ 	0.5,0.348&�6�    �� TextAlignment$ Center=�6�   $��18(�6�   $
��Bold"�6�   �� 5   
   -   5         ��Name$ 
WhiteLabel��5      $0��*White player's time
(null for unlimited)

��6$   $
��Left=�6e   $��	40,30,0,0q�6�   $	��Top=�6�   $
��Bold"�6�   $��25(�6�   $��77��6�   $	��240��6�   �� 5   
   -   5        $ BlackTime_txt��5      $
��Left=�6)   $��23��6D   $��538,120,0,0q�6P   $
 Wrap=�6e   $
��5:05��6y   $	��Top=�6�   $	��120��6�   $ 	0.5,0.348&�6�   $ Center=�6�   $��18(�6�   $
��Bold"�6�   �� 5   
   -   5        $ 
BlackLabel��5      $/��)Black player's time
(null for unlimited)
��6$   $
��Left=�6`   $��
480,31,0,0q�6{   $	��Top=�6�   $
��Bold"�6�   $��25(�6�   $��70��6�   $	��236��6�   �� 5   
   -   5        	 ��Name$	 
Submit_btn��5      $��Submit��6%   $
��Left=�66   $��311,153,0,0q�6Q   $	��Top=�6f   $	��138��6~   $
��Bold"�6�   $��30(�6�   $��90��6�   6�   5      5      