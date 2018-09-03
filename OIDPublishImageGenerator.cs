using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DotCover
{
    enum OIDBeginBuildState : int
    {
        eBBState_OK = 0,
        eBBState_Unregistered,
        eBBState_Uninitialize,
        eBBState_Invalid_UserInfo,
        eBBState_Authenticate_Error,
        eBBState_Network_Error,
        eBBState_Server_Error,
        eBBState_ImageFileNotExist,
        eBBState_FailToOpenImageFile,
        eBBState_Unknown,
    };

    // Object Type
    enum OIDPublishObjectType : int
    {
        eOID_OT_ElementCode = 0,
        eOID_OT_PositionCode,
    };

    // Print Point Type
    enum OIDPrintPointType : int
    {
        eOID_PrintPointType_2x2 = 0,	// 2x2 Point Type
        eOID_PrintPointType_3x3 = 1,	// 3x3 Point Type
    };

    // Publish Image Type
    enum OIDPublishImageType : int
    {
        eOID_PIT_Publish_Image = 0,
        eOID_PIT_Vertor_Image,
        eOID_PIT_BG_Vertor_Image,
        eOID_PIT_Publish_BG_Image,
    }

    class OIDPublishImageGenerator
    {
        [DllImport(".\\OIDPublishImageGenerator\\OIDPublishImageGenerator.dll", CallingConvention = CallingConvention.StdCall)]
        static extern bool OID_PIG_Initialize();

        [DllImport(".\\OIDPublishImageGenerator\\OIDPublishImageGenerator.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void OID_PIG_SetUserInfo(char[] szUserName, char[] szPassword);

        [DllImport(".\\OIDPublishImageGenerator\\OIDPublishImageGenerator.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern int OID_PIG_BeginBuildPublishImage(char[] szBGImage, bool bExportPDFImage);

        [DllImport(".\\OIDPublishImageGenerator\\OIDPublishImageGenerator.dll", CallingConvention = CallingConvention.StdCall)]
        static extern bool OID_PIG_SetPublishPages(int[] arPageNumbers, int nPageCount);

        [DllImport(".\\OIDPublishImageGenerator\\OIDPublishImageGenerator.dll", CallingConvention = CallingConvention.StdCall)]
        static extern bool OID_PIG_SetStartPosition(int nPageIndex, int nXStart, int nYStart);

        [DllImport(".\\OIDPublishImageGenerator\\OIDPublishImageGenerator.dll", CallingConvention = CallingConvention.StdCall)]
        static extern bool OID_PIG_AddObjectInfo(int nPageIndex, UInt32 uIObjectIndex, UInt32[] arPointX, UInt32[] arPointY, int nPointsCount, int nZOrder, int nObjectType);

        [DllImport(".\\OIDPublishImageGenerator\\OIDPublishImageGenerator.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern bool OID_PIG_BuildPublishImage(char[] szOutputFolderPath, bool bPrintIdleCode, bool bSplitBigImage, int nPrintPointType, int nPublishImageType);

        [DllImport(".\\OIDPublishImageGenerator\\OIDPublishImageGenerator.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void OID_PIG_EndBuildPublishImage();

        [DllImport(".\\OIDPublishImageGenerator\\OIDPublishImageGenerator.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void OID_PIG_Uninitialize();

        public bool Initialize()
        {
            return OID_PIG_Initialize();
        }

        public void SetUserInfo(char[] szUserName, char[] szPassword)
        {
            OID_PIG_SetUserInfo(szUserName, szPassword);
        }

        public int BeginBuildPublishImage(char[] szBGImage, bool bExportPDFImage)
        {
            return OID_PIG_BeginBuildPublishImage(szBGImage, bExportPDFImage);
        }

        public bool SetPublishPages(int[] arPageNumbers, int nPageCount)
        {
            return OID_PIG_SetPublishPages(arPageNumbers, nPageCount);
        }

        public bool SetStartPosition(int nPageIndex, int nXStart, int nYStart)
        {
            return OID_PIG_SetStartPosition(nPageIndex, nXStart, nYStart);
        }

        public bool AddObjectInfo(int nPageIndex, UInt32 uIObjectIndex, UInt32[] arPointX, UInt32[] arPointY, int nPointsCount, int nZOrder, int nObjectType)
        {
            return OID_PIG_AddObjectInfo(nPageIndex, uIObjectIndex, arPointX, arPointY, nPointsCount, nZOrder, nObjectType);
        }

        public bool BuildPublishImage(char[] szOutputFolderPath, bool bPrintIdleCode, bool bSplitBigImage, int nPrintPointType, int nPublishImageType)
        {
            return OID_PIG_BuildPublishImage(szOutputFolderPath, bPrintIdleCode, bSplitBigImage, nPrintPointType, nPublishImageType);
        }

        public void EndBuildPublishImage()
        {
            OID_PIG_EndBuildPublishImage();
        }

        public void Uninitialize()
        {
            OID_PIG_Uninitialize();
        }
    }
}
