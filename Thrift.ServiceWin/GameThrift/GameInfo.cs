/**
 * Autogenerated by Thrift Compiler (0.9.3)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.Runtime.Serialization;
using Thrift.Protocol;
using Thrift.Transport;

namespace GameThrift
{

  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class GameInfo : TBase
  {
    private int _GameId;
    private string _GameName;

    public int GameId
    {
      get
      {
        return _GameId;
      }
      set
      {
        __isset.GameId = true;
        this._GameId = value;
      }
    }

    public string GameName
    {
      get
      {
        return _GameName;
      }
      set
      {
        __isset.GameName = true;
        this._GameName = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool GameId;
      public bool GameName;
    }

    public GameInfo() {
    }

    public void Read (TProtocol iprot)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        TField field;
        iprot.ReadStructBegin();
        while (true)
        {
          field = iprot.ReadFieldBegin();
          if (field.Type == TType.Stop) { 
            break;
          }
          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.I32) {
                GameId = iprot.ReadI32();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.String) {
                GameName = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            default: 
              TProtocolUtil.Skip(iprot, field.Type);
              break;
          }
          iprot.ReadFieldEnd();
        }
        iprot.ReadStructEnd();
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public void Write(TProtocol oprot) {
      oprot.IncrementRecursionDepth();
      try
      {
        TStruct struc = new TStruct("GameInfo");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (__isset.GameId) {
          field.Name = "GameId";
          field.Type = TType.I32;
          field.ID = 1;
          oprot.WriteFieldBegin(field);
          oprot.WriteI32(GameId);
          oprot.WriteFieldEnd();
        }
        if (GameName != null && __isset.GameName) {
          field.Name = "GameName";
          field.Type = TType.String;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(GameName);
          oprot.WriteFieldEnd();
        }
        oprot.WriteFieldStop();
        oprot.WriteStructEnd();
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override string ToString() {
      StringBuilder __sb = new StringBuilder("GameInfo(");
      bool __first = true;
      if (__isset.GameId) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("GameId: ");
        __sb.Append(GameId);
      }
      if (GameName != null && __isset.GameName) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("GameName: ");
        __sb.Append(GameName);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}
