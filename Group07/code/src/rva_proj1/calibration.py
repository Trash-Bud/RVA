import time
from pathlib import Path

import numpy as np
import cv2 as cv

from .ui import calibrate_camera

CHESSBOARD_PATTERN = (6, 9)
SLEEP_INTERVAL = 2
TARGET_POINT_COUNT = 20

CRITERIA = (cv.TERM_CRITERIA_EPS + cv.TERM_CRITERIA_MAX_ITER, 30, 0.001)


def callibrate_camera(cap: cv.VideoCapture):
    # prepare object points, like (0,0,0), (1,0,0), (2,0,0) ....,(6,5,0)
    objp = np.zeros((CHESSBOARD_PATTERN[0] * CHESSBOARD_PATTERN[1], 3), np.float32)
    objp[:, :2] = np.mgrid[0:CHESSBOARD_PATTERN[0], 0:CHESSBOARD_PATTERN[1]].T.reshape(-1, 2)
    # Arrays to store object points and image points from all the images.
    objpoints = []  # 3d point in real world space
    imgpoints = []  # 2d points in image plane.

    # Call UI message to explain how to calibrate the camera
    calibrate_camera()

    while len(objpoints) < TARGET_POINT_COUNT:
        ret, frame = cap.read()
        if not ret:
            raise Exception("Failed to calibrate camera: could not read frame")

        gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)
        # Find the chess board corners
        ret, corners = cv.findChessboardCorners(gray, CHESSBOARD_PATTERN, None)
        # If found, add object points, image points (after refining them)
        if ret:
            objpoints.append(objp)
            corners2 = cv.cornerSubPix(gray, corners, (11, 11), (-1, -1), CRITERIA)
            imgpoints.append(corners2)
            # Draw and display the corners
            cv.drawChessboardCorners(frame, CHESSBOARD_PATTERN, corners2, ret)

        cv.imshow('Camera callibration', frame)
        if cv.waitKey(1) == ord('q'):
            raise Exception("Failed to calibrate camera: canceled")

        if ret:
            time.sleep(SLEEP_INTERVAL)

    cv.destroyAllWindows()
    return cv.calibrateCamera(objpoints, imgpoints, gray.shape[::-1], None, None)


def callibrate_camera_with_cache(cap: cv.VideoCapture):
    path = Path.cwd().joinpath("camera_calibration.npz")
    if path.exists():
        print(f"Loading cached calibration values from '{path}'")
        data = np.load(path)
        ret, mtx, dist, rvecs, tvecs = data["ret"].item(), data["mtx"], data["dist"], data["rvecs"], data["tvecs"]
    else:
        ret, mtx, dist, rvecs, tvecs = callibrate_camera(cap)
        np.savez(path, ret=ret, mtx=mtx, dist=dist, rvecs=rvecs, tvecs=tvecs)

    return ret, mtx, dist, rvecs, tvecs
