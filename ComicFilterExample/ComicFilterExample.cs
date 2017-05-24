using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;


namespace OpenCVForUnityExample
{

/// <summary>
/// ComicFilter example.
/// referring to the http://dev.classmethod.jp/smartphone/opencv-manga-2/.
/// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class ComicFilterExample : MonoBehaviour
    {
        /// <summary>
        /// The gray mat.
        /// </summary>
        Mat grayMat;

        /// <summary>
        /// The line mat.
        /// </summary>
        Mat lineMat;

        /// <summary>
        /// The mask mat.
        /// </summary>
        Mat maskMat;

        /// <summary>
        /// The background mat.
        /// </summary>
        Mat bgMat;

        /// <summary>
        /// The dst mat.
        /// </summary>
        Mat dstMat;

        /// <summary>
        /// The gray pixels.
        /// </summary>
        byte[] grayPixels;

        /// <summary>
        /// The mask pixels.
        /// </summary>
        byte[] maskPixels;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The web cam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        // Use this for initialization
        void Start ()
        {
            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();
            webCamTextureToMatHelper.Init ();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper inited event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInited ()
        {
            UnityEngine.Debug.Log ("OnWebCamTextureToMatHelperInited");
        
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();
        
            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
        
            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);

            UnityEngine.Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
        

            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();
        
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }


            grayMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1);
            lineMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1);
            maskMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1);
            
            //create a striped background.
            bgMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1, new Scalar (255));
            for (int i = 0; i < bgMat.rows ()*2.5f; i=i+4) {
                Imgproc.line (bgMat, new Point (0, 0 + i), new Point (bgMat.cols (), -bgMat.cols () + i), new Scalar (0), 1);
            }
            
            dstMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1);
            
            grayPixels = new byte[grayMat.cols () * grayMat.rows () * grayMat.channels ()];
            maskPixels = new byte[maskMat.cols () * maskMat.rows () * maskMat.channels ()];
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            UnityEngine.Debug.Log ("OnWebCamTextureToMatHelperDisposed");

            grayMat.Dispose ();
            lineMat.Dispose ();
            maskMat.Dispose ();
        
            bgMat.Dispose ();
            dstMat.Dispose ();

            grayPixels = null;
            maskPixels = null;
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode){
            UnityEngine.Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        int i = 0;
        
        //void Update ()
        //{
        //    if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {

        //        Stopwatch sw = new Stopwatch();
        //        sw.Start();

        //        Mat rgbaMat = webCamTextureToMatHelper.GetMat();

        //        Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

        //        //                      Utils.webCamTextureToMat (webCamTexture, grayMat, colors);
                
        //        bgMat.copyTo (dstMat);

        //        Imgproc.GaussianBlur (grayMat, lineMat, new Size (3, 3), 0);


        //        grayMat.get (0, 0, grayPixels);

        //        for (int i = 0; i < grayPixels.Length; i++) {

        //            maskPixels [i] = 0;

        //            if (grayPixels [i] < 70) {
        //                grayPixels [i] = 0;

        //                maskPixels [i] = 1;
        //            } else if (70 <= grayPixels [i] && grayPixels [i] < 120) {
        //                grayPixels [i] = 100;

        //            } else {
        //                grayPixels [i] = 255;
        //                maskPixels [i] = 1;
        //            }
        //        }

        //        grayMat.put (0, 0, grayPixels);
        //        maskMat.put (0, 0, maskPixels);
        //        grayMat.copyTo (dstMat, maskMat);


        //        Imgproc.Canny (lineMat, lineMat, 20, 120);

        //        lineMat.copyTo (maskMat);

        //        Core.bitwise_not (lineMat, lineMat);

        //        lineMat.copyTo (dstMat, maskMat);

        //        //          Imgproc.putText (dstMat, "W:" + dstMat.width () + " H:" + dstMat.height () + " SO:" + Screen.orientation, new Point (5, dstMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (0), 2, Imgproc.LINE_AA, false);

        //        //      Imgproc.cvtColor(dstMat,rgbaMat,Imgproc.COLOR_GRAY2RGBA);
        //        //              Utils.matToTexture2D (rgbaMat, texture);

        //        Utils.matToTexture2D (dstMat, texture, webCamTextureToMatHelper.GetBufferColors());

        //        sw.Stop();
        //        //Console.WriteLine(sw.Elapsed);
        //        UnityEngine.Debug.LogFormat("thoi gian chay la {0}\n",sw.ElapsedMilliseconds);
        //    }
        //}

      
        void Update()
        {
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Mat rgbaMat = webCamTextureToMatHelper.GetMat();
                int w = rgbaMat.rows();
                int h = rgbaMat.cols();
                //UnityEngine.Debug.LogFormat("cols la {0}\n", h);
                //UnityEngine.Debug.LogFormat(" height {0}\n", rgbaMat.height());
                int topWidth = 332;
                int botWidth = 272;
                int differWidth = 332 - 272;
                int halfWidth = differWidth / 2;
                float needWidth = ((float)halfWidth / (float)topWidth) * w;
                float topWidthAfter = w - needWidth * 2;
                float fx = topWidthAfter / w;


                UnityEngine.Debug.LogFormat("needWidth tawng giam is {0}",needWidth);
                Mat src_mat = new Mat(4, 1, CvType.CV_32FC2);
                Mat dst_mat = new Mat(4, 1, CvType.CV_32FC2);
                src_mat.put(0, 0,
                         0, 0,
                         w, 0,
                         w, h,
                        0, h);
                dst_mat.put(0, 0,
                        needWidth, 0,
                         w - needWidth, 0,
                         w , h ,
                         0, h );
                //dst_mat.put(0,0,
                //       0, 2*h,
                //        w + w/2 , -h/2 ,
                //        w + w/2, h + h/2,
                //        0, h - h/2);
                Mat perspectiveTransform = Imgproc.getPerspectiveTransform(src_mat, dst_mat);
                Mat dst = new Mat();//mRgba.clone();
                Imgproc.warpPerspective(rgbaMat, dst, perspectiveTransform, rgbaMat.size());
                Imgproc.resize(dst, dst, new Size(0,0), fx, 1, 1);
                //Imgproc.resize()
                //Point center = new Point(dst.cols() / 2.0F, dst.rows() / 2.0F);
                //Mat rot_mat = Imgproc.getRotationMatrix2D(center, -90, 1.0);
                //Imgproc.warpAffine(dst, dst, rot_mat, new Size(dst.width(), dst.height()));
                Utils.matToTexture2D(dst, texture, webCamTextureToMatHelper.GetBufferColors());
                sw.Stop();
                UnityEngine.Debug.LogFormat("thoi gian chay la {0}\n", sw.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable ()
        {
            webCamTextureToMatHelper.Dispose ();
        }

        /// <summary>
        /// Raises the back button event.
        /// </summary>
        public void OnBackButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("OpenCVForUnityExample");
            #else
            Application.LoadLevel ("OpenCVForUnityExample");
            #endif
        }

        /// <summary>
        /// Raises the play button event.
        /// </summary>
        public void OnPlayButton ()
        {
            webCamTextureToMatHelper.Play ();
        }

        /// <summary>
        /// Raises the pause button event.
        /// </summary>
        public void OnPauseButton ()
        {
            webCamTextureToMatHelper.Pause ();
        }

        /// <summary>
        /// Raises the stop button event.
        /// </summary>
        public void OnStopButton ()
        {
            webCamTextureToMatHelper.Stop ();
        }

        /// <summary>
        /// Raises the change camera button event.
        /// </summary>
        public void OnChangeCameraButton ()
        {
            webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);
        }
    }
}