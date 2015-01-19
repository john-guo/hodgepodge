// camera.cpp : 定义控制台应用程序的入口点。
//
#include <windows.h>    

#include <opencv/cv.h>  
#include <opencv/cxcore.h>  
#include <opencv/highgui.h>  

extern "C"
{
	#include <libavutil/opt.h>
	#include <libavcodec/avcodec.h>
	#include <libavutil/imgutils.h>
	#include <libswscale/swscale.h>
	#include <libavformat/avformat.h>
	#include <libavutil/threadmessage.h>

	#pragma comment (lib, "avcodec.lib")
	#pragma comment (lib, "avutil.lib")
	#pragma comment (lib, "swscale.lib")
	#pragma comment (lib, "avformat.lib")
}

static int fi = 0;

static int SetFrame(AVCodecContext *context, IplImage *img, AVFrame *frame, FILE *f, AVPacket &pkt)
{
	int ret, got_output;

	av_init_packet(&pkt);
	pkt.data = NULL;    // packet data will be allocated by the encoder
	pkt.size = 0;

	IplImage *tmp = cvCreateImage(cvSize(img->width, img->height), 8, 3);
	cvCvtColor(img, tmp, CV_RGB2YCrCb);

	//int linesize[] = { img->width * 3 };
	//SwsContext *sws = sws_getContext(img->width, img->height, AV_PIX_FMT_BGR24,
	//	frame->width, frame->height, (AVPixelFormat)frame->format, SWS_ACCURATE_RND, NULL, NULL, NULL);
	//sws_scale(sws, (const uint8_t* const*)img->imageData, linesize, 0, img->height,
	//	frame->data, frame->linesize);

	int idx_in = 0;
	int idx_out_y = 0;
	int idx_out_u = 0;
	int idx_out_v = 0;
	
	for (int j = 0; j < img->height; j++) {
		idx_in = j * img->widthStep;
		for (int i = 0; i < img->widthStep; i += 12) {
			// We use the chroma sample here, and put it into the out buffer 
			// take the luminance sample 
			frame->data[0][idx_out_y] = tmp->imageData[idx_in + i + 0]; // Y 
			idx_out_y++;
			frame->data[0][idx_out_y] = tmp->imageData[idx_in + i + 3]; // Y 
			idx_out_y++;
			frame->data[0][idx_out_y] = tmp->imageData[idx_in + i + 6]; // Y 
			idx_out_y++;
			frame->data[0][idx_out_y] = tmp->imageData[idx_in + i + 9]; // Y 
			idx_out_y++;
			if ((j % 2) == 0) {
				// take the blue-difference and red-difference chroma components sample  
				frame->data[1][idx_out_u++] = tmp->imageData[idx_in + i + 1]; // Cr U  
				frame->data[1][idx_out_u++] = tmp->imageData[idx_in + i + 7]; // Cr U  
				frame->data[2][idx_out_v++] = tmp->imageData[idx_in + i + 2]; // Cb V  
				frame->data[2][idx_out_v++] = tmp->imageData[idx_in + i + 8]; // Cb V 

			}
		}
	}

	cvRelease((void**)&tmp);

	frame->pts = fi++;


	/* encode the image */
	ret = avcodec_encode_video2(context, &pkt, frame, &got_output);
	if (ret < 0) {
		fprintf(stderr, "Error encoding frame\n");
		exit(1);
	}

	if (got_output) {
		printf("Write frame %3d (size=%5d)\n", fi, pkt.size);
		//pkt.pts = frame->pts;
		//fwrite(pkt.data, 1, pkt.size, f);
		return 0;
		//av_free_packet(&pkt);
	}

	return 1;
}


static void begin_video_encode(const char *filename, AVCodecID codec_id, AVCodecContext *&c, FILE *&f, AVFrame *&frame)
{
	AVCodec *codec;
	int ret;
	
	printf("Encode video file %s\n", filename);

	/* find the mpeg1 video encoder */
	codec = avcodec_find_encoder(codec_id);
	if (!codec) {
		fprintf(stderr, "Codec not found\n");
		exit(1);
	}

	c = avcodec_alloc_context3(codec);
	if (!c) {
		fprintf(stderr, "Could not allocate video codec context\n");
		exit(1);
	}

	/* put sample parameters */
	c->bit_rate = 400000;
	/* resolution must be a multiple of two */
	c->width = 640;
	c->height = 480;
	/* frames per second */
	c->time_base = { 1, 20 };
	/* emit one intra frame every ten frames
	* check frame pict_type before passing frame
	* to encoder, if frame->pict_type is AV_PICTURE_TYPE_I
	* then gop_size is ignored and the output of encoder
	* will always be I frame irrespective to gop_size
	*/
	c->gop_size = 10;
	c->max_b_frames = 1;
	c->pix_fmt = AV_PIX_FMT_YUV420P;

	if (codec_id == AV_CODEC_ID_H264)
		av_opt_set(c->priv_data, "preset", "slow", 0);

	/* open it */
	if (avcodec_open2(c, codec, NULL) < 0) {
		fprintf(stderr, "Could not open codec\n");
		exit(1);
	}

	f = fopen(filename, "wb");
	if (!f) {
		fprintf(stderr, "Could not open %s\n", filename);
		exit(1);
	}

	frame = av_frame_alloc();
	if (!frame) {
		fprintf(stderr, "Could not allocate video frame\n");
		exit(1);
	}
	frame->format = c->pix_fmt;
	frame->width = c->width;
	frame->height = c->height;

	/* the image can be allocated by any means and av_image_alloc() is
	* just the most convenient way if av_malloc() is to be used */
	ret = av_image_alloc(frame->data, frame->linesize, c->width, c->height,
		c->pix_fmt, 32);
	if (ret < 0) {
		fprintf(stderr, "Could not allocate raw picture buffer\n");
		exit(1);
	}
}


static void end_video_encode(AVCodecContext *c, FILE *f, AVFrame *frame, AVPacket &pkt)
{
	int ret, got_output;
	uint8_t endcode[] = { 0, 0, 1, 0xb7 };
	
	/* get the delayed frames */
	for (got_output = 1; got_output; fi++) {
		fflush(stdout);

		av_init_packet(&pkt);
		pkt.data = NULL;    // packet data will be allocated by the encoder
		pkt.size = 0;

		ret = avcodec_encode_video2(c, &pkt, NULL, &got_output);
		if (ret < 0) {
			fprintf(stderr, "Error encoding frame\n");
			exit(1);
		}

		if (got_output) {
			printf("Write frame %3d (size=%5d)\n", fi, pkt.size);
			//fwrite(pkt.data, 1, pkt.size, f);
			av_free_packet(&pkt);
		}
	}

	/* add sequence end code to have a real mpeg file */
	//fwrite(endcode, 1, sizeof(endcode), f);
	//fclose(f);

	avcodec_close(c);
	av_free(c);
	av_freep(&frame->data[0]);
	av_frame_free(&frame);
	printf("\n");
}

static AVThreadMessageQueue *queue;
static AVFormatContext *fc;
static volatile int quit = 0;
static AVStream *stream;

DWORD WINAPI ThreadWorker(LPVOID pM)
{
	AVPacket p;

	while (!quit)
	{
		av_thread_message_queue_recv(queue, &p, 0);
		//av_interleaved_write_frame(fc, &p);
		av_packet_rescale_ts(&p, stream->codec->time_base, stream->time_base);
		p.stream_index = stream->index;

		//av_interleaved_write_frame(fc, &p);
		av_write_frame(fc, &p);
		av_free_packet(&p);
	}

	if (av_thread_message_queue_recv(queue, &p, AV_THREAD_MESSAGE_NONBLOCK) == 0)
		av_free_packet(&p);

	quit++;

	return 0;
}

static int(*writer)(AVFormatContext *) = NULL;
static int my_write_header(AVFormatContext *fc)
{
	int ret = writer(fc);
	return ret;
}

int main(int argc, char* argv[])
{
	av_register_all();
	avcodec_register_all();
	avformat_network_init();

	const char *filename = "rtsp://localhost/test";
	av_thread_message_queue_alloc(&queue, 100, sizeof(AVPacket));
	int ret = avformat_alloc_output_context2(&fc, NULL, "RTSP", filename);
	if (ret < 0)
	{
		printf("avformat_alloc_output_context2 failed: %s\n");
		return -1;
	}
	
	HANDLE thread = CreateThread(NULL, 0, ThreadWorker, NULL, 0, NULL);

	IplImage *img = NULL;
	CvCapture *pCapture = cvCreateCameraCapture(0);
	cvNamedWindow("video", 1);
	
	if (cvGrabFrame(pCapture)) {
		img = cvRetrieveFrame(pCapture);
	}
	clock_t begin = clock();
	int rate = 0;
	do {
		rate++;
		cvQueryFrame(pCapture);
	} while ((clock() - begin) < CLOCKS_PER_SEC);

	AVCodecContext *c;
	FILE *f = NULL;
	AVFrame *frame;
	
	AVCodec *codec = avcodec_find_encoder(fc->oformat->video_codec);

	stream = avformat_new_stream(fc, codec);
	c = stream->codec;

	c->bit_rate = 400000;
	c->width = img ? img->width : 640;
	c->height = img ? img->height : 480;
	stream->time_base = c->time_base = { 1, rate };
	c->gop_size = 1;
	//c->max_b_frames = 0;
	c->pix_fmt = AV_PIX_FMT_YUV420P;
	c->flags |= CODEC_FLAG_GLOBAL_HEADER;
	if (fc->oformat->video_codec == AV_CODEC_ID_H264)
		av_opt_set(c->priv_data, "preset", "slow", 0);

	if (avcodec_open2(c, codec, NULL) < 0) {
		fprintf(stderr, "Could not open codec\n");
		exit(1);
	}

	frame = av_frame_alloc();
	if (!frame) {
		fprintf(stderr, "Could not allocate video frame\n");
		exit(1);
	}
	frame->format = c->pix_fmt;
	frame->width = c->width;
	frame->height = c->height;

	ret = av_frame_get_buffer(frame, 32);
	if (ret < 0) {
		fprintf(stderr, "Could not allocate raw picture buffer\n");
		exit(1);
	}

	//av_dump_format(fc, 0, filename, 1);

	if (!(fc->oformat->flags & AVFMT_NOFILE)) {
		ret = avio_open(&fc->pb, filename, AVIO_FLAG_WRITE);
		if (ret < 0) {
			fprintf(stderr, "Could not open '%s'\n", filename);
			return 1;
		}
	}

	ret = avformat_write_header(fc, NULL);
	if (ret < 0)
	{
		fprintf(stderr, "avformat_write_header failed.\n");
		return 1;
	}

	while (!quit)
	{
		img = cvQueryFrame(pCapture);
		if (!img) continue;
		cvShowImage("video", img);
		if (cvWaitKey(33) == 27)
		{
			quit++;
		}

		AVPacket pkt = { 0 };

		int got_output;

		av_init_packet(&pkt);

		IplImage *tmp = cvCreateImage(cvSize(img->width, img->height), 8, 3);
		cvCvtColor(img, tmp, CV_RGB2YCrCb);

		int idx_in = 0;
		int idx_out_y = 0;
		int idx_out_u = 0;
		int idx_out_v = 0;

		for (int j = 0; j < img->height; j++) {
			idx_in = j * img->widthStep;
			for (int i = 0; i < img->widthStep; i += 12) {
				// We use the chroma sample here, and put it into the out buffer 
				// take the luminance sample 
				frame->data[0][idx_out_y] = tmp->imageData[idx_in + i + 0]; // Y 
				idx_out_y++;
				frame->data[0][idx_out_y] = tmp->imageData[idx_in + i + 3]; // Y 
				idx_out_y++;
				frame->data[0][idx_out_y] = tmp->imageData[idx_in + i + 6]; // Y 
				idx_out_y++;
				frame->data[0][idx_out_y] = tmp->imageData[idx_in + i + 9]; // Y 
				idx_out_y++;
				if ((j % 2) == 0) {
					// take the blue-difference and red-difference chroma components sample  
					frame->data[1][idx_out_u++] = tmp->imageData[idx_in + i + 1]; // Cr U  
					frame->data[1][idx_out_u++] = tmp->imageData[idx_in + i + 7]; // Cr U  
					frame->data[2][idx_out_v++] = tmp->imageData[idx_in + i + 2]; // Cb V  
					frame->data[2][idx_out_v++] = tmp->imageData[idx_in + i + 8]; // Cb V 

				}
			}
		}

		cvRelease((void**)&tmp);

		frame->pts = fi++;

		/* encode the image */
		ret = avcodec_encode_video2(stream->codec, &pkt, frame, &got_output);
		if (ret < 0) {
			fprintf(stderr, "Error encoding frame\n");
			exit(1);
		}


		if (got_output)
		{
			//printf("Write frame %3d (size=%5d)\n", fi, pkt.size);
			av_dup_packet(&pkt);
			av_thread_message_queue_send(queue, &pkt, 0);
		}
	}

	while (quit < 2);

	av_write_trailer(fc);

	cvReleaseCapture(&pCapture);
	cvDestroyWindow("video");

	avcodec_close(stream->codec);
	av_frame_free(&frame);
	
	avio_close(fc->pb);
	avformat_free_context(fc);

	av_thread_message_queue_free(&queue);
	avformat_network_deinit();
}

