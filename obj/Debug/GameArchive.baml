   M S B A M L   `   `   `  ����U  QProject_ChessWithInterface, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:Project_ChessWithInterfaceProject_ChessWithInterface  5   	   ,    &Project_ChessWithInterface.GameArchive   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   :local(clr-namespace:Project_ChessWithInterface     5   	   -   5          �Name$  GameArchiveWindow��5   	    � Title$ GameArchive��5   	   $	��622��6   $	��893��6*    � 
ResizeMode$ NoResize=�6M    � WindowStartupLocation$ CenterScreen=�6c   .��5	      � .+�5
   
   �� -   5         ��Name$ 	Games_lst��5
      $
��Left=�6%   $	��570��6@   $��5,5,0,0q�6M   $	��Top=�6^   $	��430��6v   $��16(�6�   $
��Bold"�6�   6�   �� 5   
   -   5         ��Name$ 	Reset_btn��5      $��Reset��6$   $
��Left=�64   $��
591,15,0,0q�6O   $	��Top=�6c   $	��131��6{   $
��Bold"�6�   $��30(�6�   $��79��6�   6�   �� 5   
   -   5         ��Name$ label��5      $%��Search by nickname
(or color):��6   $
��Left=�6Q   $��490,111,0,0q�6l   $	��Top=�6�    J� RenderTransformOrigin$ 0.368,0.577&�6�   $
��Bold"�6�   $��20(�6�   �� 5   
   -   5         ��Name$ NameSearch_txt��5      $
��Left=�6*   $��27��6E   $��706,130,0,0q�6Q   	 �� TextWrapping$
	 Wrap=�6f   $	��Top=�6z   $	��152��6�   $��18(�6�   $
��Bold"�6�   �� 5   
   -   5        $ 
label_Copy��5      $%��Search by date:
(YYYY-MM-DD)
��6$   $
��Left=�6^   $��491,183,0,0q�6y   $	��Top=�6�   $ 0.368,0.577&�6�   $
��Bold"�6�   $��20(�6�   �� 5   
   -   5        $ DateSearch_txt��5      $
��Left=�6*   $��27��6E   $��706,199,0,0q�6Q   $
	 Wrap=�6f   $�� ��6z   $	��Top=�6�   $	��152��6�   $��18(�6�   $
��Bold"�6�   �� 5   
   -   5        $ label_Copy1��5      $$��Search by move 
count (>10):

��6%   $
��Left=�6]   $��487,250,0,0q�6x   $	��Top=�6�   $ 0.368,0.577&�6�   $
��Bold"�6�   $��20(�6�   $	��188��6�   $��72��6�   �� 5   
   -	   5        $ MoveCountSearch_txt��5      $
��Left=�6/   $��27��6J   $��706,270,0,0q�6V   $
	 Wrap=�6k   $�� ��6   $	��Top=�6�   $	��152��6�   $��18(�6�   $
��Bold"�6�   �� 5   
   -
   5        $ 
Search_btn��5      $��Search��6%   $
��Left=�66   $��610,358,0,0q�6Q   $	��Top=�6f   $	��137��6~   $
��Bold"�6�   $��30(�6�   $	��118��6�   6�   �� 5   
   -   5        $ DisplayRecord_btn��5      $�� ��6,   $
��Left=�67   $��
735,15,0,0q�6R   $	��Top=�6f   $	��129��6~   $
��Bold"�6�   $��30(�6�   $��78��6�   6�   �� 5   
   -   5        $ Back_btn��5      $
��Back��6#   $
��Left=�62   $��
448,15,0,0q�6M   $	��Top=�6a   $	��130��6y   $
��Bold"�6�   $��30(�6�   $��80��6�   6�   5      5      