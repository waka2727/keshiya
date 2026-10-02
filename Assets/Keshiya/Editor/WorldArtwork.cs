using System.Collections.Generic;
using UnityEngine;
namespace Keshiya.Editor {
 // Deliberately simple pen strokes, baked into JobPath assets (no runtime font dependency).
 public static class WorldArtwork {
  static readonly Dictionary<char,string> letters=new Dictionary<char,string>{
   {'A',"00 26 40/13 33"},{'B',"00 06 36 45 44 33 03/33 42 41 30 00"},{'C',"46 06 00 40"},{'D',"00 06 26 45 41 20 00"},{'E',"46 06 00 40/03 33"},{'F',"00 06 46/03 33"},{'G',"46 06 00 40 43 23"},{'H',"00 06/40 46/03 43"},{'I',"06 46/26 20/00 40"},{'J',"06 46 40 00 02"},{'K',"00 06/46 03 40"},{'L',"06 00 40"},{'M',"00 06 23 46 40"},{'N',"00 06 40 46"},{'O',"00 06 46 40 00"},{'P',"00 06 46 43 03"},{'R',"00 06 46 43 03/23 40"},{'S',"46 06 03 43 40 00"},{'T',"06 46/26 20"},{'U',"06 00 40 46"},{'V',"06 20 46"},{'W',"06 10 23 30 46"},{'X',"06 40/00 46"},{'Y',"06 23 46/23 20"},{'Z',"06 46 00 40"},
   {'0',"00 06 46 40 00"},{'1',"15 26 20/10 30"},{'2',"06 46 43 03 00 40"},{'3',"06 46 40 00/03 43"},{'4',"06 03 43/46 40"},{'5',"46 06 03 43 40 00"},{'6',"46 06 00 40 43 03"},{'7',"06 46 20"},{'8',"00 06 46 40 00/03 43"},{'9',"40 46 06 03 43"},{'+',"03 43/20 26"},{'=',"02 42/04 44"}
  };
  public static void Text(List<JobPath> into,string text,float x,float y,float unit=.07f){foreach(char letter in text){if(letters.TryGetValue(letter,out var glyph))foreach(var stroke in glyph.Split('/')){var tokens=stroke.Split(' ');var points=new Vector2[tokens.Length];for(int i=0;i<points.Length;i++)points[i]=new Vector2(x+(tokens[i][0]-'0')*unit,y+(tokens[i][1]-'0')*unit);into.Add(new JobPath(points));}x+=unit*6;}}
  public static void Line(List<JobPath> into,float x,float y,float xx,float yy)=>into.Add(new JobPath(new[]{new Vector2(x,y),new Vector2(xx,yy)}));
  public static void Box(List<JobPath> into,float x,float y,float w,float h)=>into.Add(new JobPath(new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h),new Vector2(x,y)}));
 }
}
