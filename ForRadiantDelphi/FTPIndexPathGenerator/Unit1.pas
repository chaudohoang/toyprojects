unit Unit1;

interface

uses
  Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants, System.Classes, Vcl.Graphics,
  Vcl.Controls, Vcl.Forms, Vcl.Dialogs, Vcl.StdCtrls, System.Math;

type
  TForm1 = class(TForm)
    Button1: TButton;
    Label1: TLabel;
    Edit1: TEdit;
    Label2: TLabel;
    Edit2: TEdit;
    procedure Button1Click(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  Form1: TForm1;
  prSeed,LayerCount,LayerSize: Integer;

implementation

{$R *.dfm}

function GetDfsHashValue(pKeyStr: String): Integer;
var
  i, tmpVal, strLength  : Integer;
  lTemp : Int64;
begin
  prSeed      := 7919;
  strLength := Length(pKeyStr);
  if strLength = 0 then begin
    Result := 0;
    Exit;
  end;
  //
  tmpVal := 0;
  for i := 0 to strLength - 1 do begin
    lTemp   := tmpVal;
    lTemp   := lTemp * $ff;
    lTemp   := lTemp + ($ff and Ord(pKeyStr[i+1]));
    tmpVal  := lTemp mod prSeed;
  end;
  Result := tmpVal;
end;

function TranHashValue2NumberInLayer(hashValue, layerNumber: Integer): Double;
var
  functionReturnValue : Double;
begin
  if layerNumber = 0 then begin
    functionReturnValue := hashValue / LayerSize;
    functionReturnValue := functionReturnValue - 0.49999;
  end
  else begin
    functionReturnValue := hashValue mod LayerSize;
  end;

  Result := functionReturnValue;
end;

function GetDfsHashPath(sPanelId: String): String;
var
  IndexFilePath : string;
  dTemp         : Double;
  nDfsHashValue, nTemp : Integer;
begin
  try
    prSeed      := 7919;  // 1021  --> 7919
    LayerCount  := 1;
    if prSeed <= 157 then begin
      LayerSize     := prSeed;
    end
    else begin
      LayerCount  := 2;
      nTemp       := prSeed;
      LayerSize   := Trunc(nTemp / (Trunc(Power(prSeed, 0.5))));
    end;
    //
    nDfsHashValue := GetDfsHashValue(sPanelId);
    if LayerCount = 1 then begin
      dTemp         := TranHashValue2NumberInLayer(nDfsHashValue, 1);
      IndexFilePath := IndexFilePath + FormatFloat('00000000', dTemp);
    end
    else begin
      dTemp         := TranHashValue2NumberInLayer(nDfsHashValue, 0);
      IndexFilePath := IndexFilePath + FormatFloat('00000000', dTemp) + '\';
      dTemp         := TranHashValue2NumberInLayer(nDfsHashValue, 1);
      IndexFilePath := IndexFilePath + FormatFloat('00000000', dTemp);
    end;
    Result := IndexFilePath;
  except
    Result := '';
  end;
end;



procedure TForm1.Button1Click(Sender: TObject);
begin
  Edit2.Text:= GetDfsHashPath(Edit1.Text);
end;

end.
