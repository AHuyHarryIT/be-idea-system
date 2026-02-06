namespace IdeaCollectionIdea.Common.Constants
{
    public class MaxLengths
    {
        // Generic

        public const int NAME = 100;
        public const int TITLE = 200;

        // Text content

        public const int SUMMARY = 1000;
        public const int DESCRIPTION = 4000;
        public const int TEXT = 2000;
        public const int COMMENT = 3000;

        // Identity standard
        public const int EMAIL_ADDRESS = 256;
        public const int PHONE_NUMBER = 20;
        public const int HASH_PASSWORD = 500;


        // Media
        public const int FILE_PATH = 500;
        public const int FILE_FORMAT = 20;
        public const int QUALITY = 20;            // 720p, 1080p, 4K
        public const int SERVER_NAME = 50;        // CDN, VIP, Backup


        // URLs
        public const int URL = 500;
        public const int IMAGE_URL = 500;
        public const int VIDEO_URL = 500;
        public const int TRAILER_URL = 500;

    }
}