object Form1: TForm1
  Left = 0
  Top = 0
  Caption = 'FTPIndexPathGenerator'
  ClientHeight = 299
  ClientWidth = 635
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object Label1: TLabel
    Left = 24
    Top = 16
    Width = 17
    Height = 13
    Caption = 'PID'
  end
  object Label2: TLabel
    Left = 24
    Top = 74
    Width = 22
    Height = 13
    Caption = 'Path'
  end
  object Button1: TButton
    Left = 24
    Top = 40
    Width = 177
    Height = 25
    Caption = 'Generate'
    TabOrder = 0
    OnClick = Button1Click
  end
  object Edit1: TEdit
    Left = 80
    Top = 13
    Width = 121
    Height = 21
    TabOrder = 1
    Text = 'GVC13020QJR15031Y'
  end
  object Edit2: TEdit
    Left = 80
    Top = 71
    Width = 121
    Height = 21
    TabOrder = 2
  end
end
