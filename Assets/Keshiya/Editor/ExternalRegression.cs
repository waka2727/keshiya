using System;
using System.IO;
using UnityEngine;
namespace Keshiya.Editor {
 public static class ExternalRegression {
  public static void Run(){int failures=0;Directory.CreateDirectory("TestResults-External");string path="TestResults-External/regression.txt";File.WriteAllText(path,"Regression run "+DateTime.UtcNow.ToString("O")+"\n");Action[] checks={PrototypeChecks.Run,FeelChecks.Run,ToolChecks.Run,RoleChecks.Run,EconomyChecks.Run,ZigzagChecks.Run,WorkChecks.Run,SkillChecks.Run,ShopChecks.Run,BalanceChecks.Run,SpecialistChecks.Run,WorldChecks.Run};foreach(var check in checks){try{check();File.AppendAllText(path,"PASS "+check.Method.DeclaringType.Name+"\n");}catch(Exception e){failures++;File.AppendAllText(path,"FAIL "+check.Method.DeclaringType.Name+": "+e+"\n");Debug.LogException(e);}}File.AppendAllText(path,"Failures="+failures);if(failures>0)UnityEditor.EditorApplication.Exit(1);}
 }
}
