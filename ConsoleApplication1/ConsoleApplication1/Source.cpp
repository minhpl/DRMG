#include "opencv2/imgproc.hpp"
#include "opencv2/highgui.hpp"
#include <iostream>
#include <ctime>
#include <chrono>

using namespace cv;
int main(int, char**)
{
	VideoCapture cap(0);
	if (!cap.isOpened()) return -1;
	Mat frame, edges;
	namedWindow("edges", 1);

	for (;;)
	{
		cap >> frame;
		//cvtColor(frame, edges, COLOR_BGR2GRAY);
		//GaussianBlur(edges, edges, Size(7, 7), 1.5, 1.5);
		//Canny(edges, edges, 0, 30, 3);
		
		int h = frame.rows;
		int w = frame.cols;

		float src_data[10] = { 0, 0,
			0, 0,
			w, 0,
			w, h,
			0, h };

		float dst_data[10] = { 0, 0,
			0, h / 8,
			w + w / 4, -h / 8,
			w + w / 4, h + h / 8,
			0, h - h / 8 };

		cv::Mat src_mat = cv::Mat(4, 1, CV_32FC2, src_data, 0);
		cv::Mat dst_mat = cv::Mat(4, 1, CV_32FC2, dst_data, 0);

		cv::Mat perspectiveTransform = cv::getPerspectiveTransform(src_mat, dst_mat);

		//int start_s = clock();
		auto start = std::chrono::high_resolution_clock::now();
		cv::warpPerspective(src_mat, dst_mat, perspectiveTransform, src_mat.size());
		//int stop_s = clock();
		auto end = std::chrono::high_resolution_clock::now();
		auto elapsedNano = std::chrono::duration_cast<std::chrono::nanoseconds>(end - start);

		auto elapsedMili = std::chrono::duration_cast<std::chrono::milliseconds>(end - start);

		std::cout << "time: " << elapsedNano.count() <<" nano seconds" <<std::endl;
		std::cout << "time: " << elapsedMili.count() << " mili seconds" << std::endl;

		cv::Point center = cv::Point(dst_mat.cols / 2.0F, dst_mat.rows / 2.0F);
		cv::Mat rot_mat = cv::getRotationMatrix2D(center, -90, 1.0);
		cv::warpAffine(dst_mat, dst_mat, rot_mat, cv::Size(dst_mat.rows, dst_mat.cols));


		imshow("edges", dst_mat);

		if (waitKey(30) >= 0) continue;
	}

	return 0;
}