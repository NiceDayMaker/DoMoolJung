   M S B A M L   `   `   `  ����E  ADoMoolJung, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:DoMoolJung
DoMoolJung  5   	       DoMoolJung.MainWindow   5   	   W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:DoMoolJung     5   	   -   5          � Title$*  $ㄷ井 두물정 1.0 - 이름없음*��5	   	    � WindowState$ 	Maximized=�6$   ���#4c5052$	��200��6Q   $	��350��6a   $	��600��6p   $	��500��6}   .��5      �  � RowDefinitions 5   
   �� 5      $��30 �6   �� 5      $��1* �6   �� 5      $��50 �6    � ColumnDefinitions 5   
   �� 5      -   5         ��Name$ 
LinerWidth��5      $��50 �63   �� 5      $��1* �6   .+�5   
   �� -   5         ��Name$ Compile��5      $��
  실행  ��6"   $��Right=�63   $��3��6O   $��1��6\   $��Center=�6l   $��0,0,30,0q�6�   6�   �� 5   
   -   5         ��Name$ Source��5      $+��%/DoMoolJung;component/Fonts/#D2Coding)�5      $��1��5      $��0��5      $��2��5      $��	10,0,10,0q�5      $��40,1,0,0q�5      $0��*주석 여기에 코드를 작성하세요��5      ���#3c3f41
�������$��15(�5"       �� AcceptsReturn$
 True��5#       �� 
AcceptsTab$
 True��5$      $
��True��5%      $��Visible=�5&      $��Visible=�5'      5(      �� 5)   
   -   5        $ Liner��5*      $+��%/DoMoolJung;component/Fonts/#D2Coding)�5+      $��Stretch=�5,      $��	10,0,0,20q�5-      $��0,1,1,0q�5.      	 �� TextWrapping$	 NoWrap=�5/      
 � Cursor$
  ��50       �� 
CaretBrush
 �����$��1��52      $��Stretch=�53      ���#2c2f31
�������$��1��56      $��0��57      $
��True��58      $��Disabled=�59      $��Hidden=�5:       �� TextAlignment$ Right=�5;      ${�False��5<      $��15(�5=      � 5>   
    � Orientation$ 
Horizontal=�6   $��0��6-   $��2��6=   $��0,0,0,10q�6Q   ���#2c2f31.+�5?      �� -   5        $
 Open��5?      $��
  열기  ��6#   $
��Left=�64   $��Center=�6O   ���#2c2f31
�������6�   �� 5@      -   5        $
 Save��5@      $��
  저장  ��6#   $
��Left=�64   $��Center=�6O   ���#2c2f31
�������6�   �� 5A      -   5        $
 Help��5A      $��  도움말  ��6#   $
��Left=�65   $��Center=�6P   ���#2c2f31
�������6�   5B      �� 5C   
   -	   5         ��Name$ Scale��5C      $
��100%��6   $
��Left=�6.   $��15��6I   $��50��6U   $��15,0,0,0q�6`   $��0q�6r   $��2��6~   
�������$��0��6�   $��2��6�   $��12(�6�   $	��Top=�6�   5D      5E      