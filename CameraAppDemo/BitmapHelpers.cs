using Android.Content;
using Android.Media;
using Android.Net;
using Android.Provider;
using Android.Widget;
using Java.Lang;
using Orientation = Android.Media.Orientation;

namespace CameraAppDemo
{
    using System.IO;
    using Android.Graphics;
    using Android.Graphics.Drawables;

    public static class BitmapHelpers
    {
        /// <summary>
        /// This method will recyle the memory help by a bitmap in an ImageView
        /// </summary>
        /// <param name="imageView">Image view.</param>
        public static void RecycleBitmap(this ImageView imageView)
        {
            var toRecycle = imageView?.Drawable;
            ((BitmapDrawable) toRecycle)?.Bitmap?.Recycle();
        }


        /// <summary>
        /// Load the image from the device, and resize it to the specified dimensions.
        /// </summary>
        /// <returns>The and resize bitmap.</returns>
        /// <param name="fileName">File name.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options
            {
                // InBitmap = true,
                InJustDecodeBounds = true
            };
            BitmapFactory.DecodeFile(fileName, options);

            ExifInterface exif = new ExifInterface(fileName);
            var orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 1);
            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            var outHeight = options.OutHeight;
            var outWidth = options.OutWidth;
            var inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                    ? outHeight / height
                    : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            // options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            var resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            return ExifRotateBitmap(fileName, resizedBitmap);
            
            // return resizedBitmap;
        }

        private static Bitmap ExifRotateBitmap(string filePath, Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            var exif = new ExifInterface(filePath);
            var rotation = exif.GetAttributeInt(ExifInterface.TagOrientation, (int) Orientation.Normal);
            var rotationInDegrees = ExifToDegrees(rotation);
            if (rotationInDegrees == 0)
                return bitmap;

            using var matrix = new Matrix();
            matrix.PreRotate(rotationInDegrees);
            return Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
        }

        private static int ExifToDegrees(int exifOrientation)
        {
            return exifOrientation switch
            {
                (int) Orientation.Rotate90 => 90,
                (int) Orientation.Rotate180 => 180,
                (int) Orientation.Rotate270 => 270,
                _ => 0
            };
        }

        // public static Bitmap getCorrectlyOrientedImage(Context context, Uri photoUri)
        // {
        //     var stream = context.ContentResolver.OpenInputStream(photoUri);
        //     BitmapFactory.Options dbo = new BitmapFactory.Options();
        //     dbo.InJustDecodeBounds = true;
        //     BitmapFactory.DecodeStream(stream, null, dbo);
        //     stream.Close();
        //
        //     int rotatedWidth, rotatedHeight;
        //     int orientation = getOrientation(context, photoUri);
        //
        //     if (orientation == 90 || orientation == 270)
        //     {
        //         rotatedWidth = dbo.OutHeight;
        //         rotatedHeight = dbo.OutWidth;
        //     }
        //     else
        //     {
        //         rotatedWidth = dbo.OutWidth;
        //         rotatedHeight = dbo.OutHeight;
        //     }
        //
        //     Bitmap srcBitmap;
        //     stream = context.ContentResolver.OpenInputStream(photoUri);
        //     if (rotatedWidth > MAX_IMAGE_DIMENSION || rotatedHeight > MAX_IMAGE_DIMENSION)
        //     {
        //         float widthRatio = ((float) rotatedWidth) / ((float) MAX_IMAGE_DIMENSION);
        //         float heightRatio = ((float) rotatedHeight) / ((float) MAX_IMAGE_DIMENSION);
        //         float maxRatio = Math.Max(widthRatio, heightRatio);
        //
        //         // Create the bitmap from file
        //         BitmapFactory.Options options = new BitmapFactory.Options();
        //         options.InSampleSize = (int) maxRatio;
        //         srcBitmap = BitmapFactory.DecodeStream(stream, null, options);
        //     }
        //     else
        //     {
        //         srcBitmap = BitmapFactory.DecodeStream(stream);
        //     }
        //
        //     stream.Close();
        //
        //     /*
        //      * if the orientation is not 0 (or -1, which means we don't know), we
        //      * have to do a rotation.
        //      */
        //     if (orientation > 0)
        //     {
        //         Matrix matrix = new Matrix();
        //         matrix.PostRotate(orientation);
        //
        //         srcBitmap = Bitmap.CreateBitmap(srcBitmap, 0, 0, srcBitmap.Width,
        //             srcBitmap.Height, matrix, true);
        //     }
        //
        //     return srcBitmap;
        // }
    }
}