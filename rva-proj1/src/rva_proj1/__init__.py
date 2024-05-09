# from threading import Thread
# import time

import cv2
# import numpy

from .ui import Quit, choose_marker_type
from .calibration import callibrate_camera_with_cache
from .marker import GAUSSIAN_KERNEL_SIZE, define_obj_pts, filter_contours, find_best_match, find_homography
# from .renderer import Renderer
from .sword import draw_sword_cube, draw_sword_flat


def main():
    # Initialize marker type as cube
    useFlatSword = False

    # Initialize camera
    cap = cv2.VideoCapture(0)

    # Check if camera opened successfully
    if not cap.isOpened():
        print("Cannot open camera")
        return

    # Calibrate camera
    _, cal_mtx, cal_dist, _, _ = callibrate_camera_with_cache(cap)

    try:
        # Choose marker type
        useFlatSword = choose_marker_type()
    except Quit as e:  # If user pressed 'q' key or closed the window
        print(e)  # Print exception message
        return  # Exit program

    # Initialize the OpenGL renderer in a separate thread
    # Renderer.window_size = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    # render_thread = Thread(target=Renderer.run, name="render", daemon=True)
    # render_thread.start()
    #
    # while True:
    #     try:
    #         window = moderngl_window.window()
    #         if window.config is None:
    #             time.sleep(0.1)
    #             continue
    #         break
    #     except ValueError:
    #         time.sleep(0.1)

    # Read until camera is closed
    while True:
        # if window.is_closing:
        #     break

        # Capture frame-by-frame
        ret, frame = cap.read()
        # If the frame is not read correctly, break the loop
        if not ret:
            print("Can't receive frame (stream end?). Exiting...")
            break

        # Send the frame to the OpenGL renderer
        # rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        # flipped_rgb_frame = numpy.flip(rgb_frame, 0).copy(order='C')
        # window.config.update_frame(flipped_rgb_frame)

        # Operations on the frame
        # Convert to grayscale
        gray_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        # Blur image to remove noise
        gaussian_frame = cv2.GaussianBlur(gray_frame, (GAUSSIAN_KERNEL_SIZE, GAUSSIAN_KERNEL_SIZE), 0)
        # Threshold image
        ret, threshold_frame = cv2.threshold(gaussian_frame, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        # Find contours
        contours, _ = cv2.findContours(threshold_frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

        # Filter contours
        output = filter_contours(contours, threshold_frame)

        # found_homography = False

        # Draw contours and sword
        for contour, _, box in output:
            # Draw contours
            cv2.drawContours(frame, [contour], -1, (255, 0, 0), 5)
            # Find the best match for the marker found
            best_marker, rotation, _ = find_best_match(output)

            # If a marker was found
            if best_marker is not None:
                # Send the homography data to the OpenGL renderer
                # h, status = find_homography(box, rotation)
                # window.config.update_homography(h)
                # found_homography = True

                # Set object points for the marker's rotation
                objtp = define_obj_pts(rotation)

                # Convert both image points and object points to float32
                objtp = objtp.astype('float32')
                imgp = box.astype('float32')

                # Solve PnP to get rotation and translation vectors
                _, rvecs, tvecs = cv2.solvePnP(objtp, imgp, cal_mtx, cal_dist)

                # Draw sword depending on the marker type
                if useFlatSword:
                    draw_sword_flat(objtp, cal_mtx, cal_dist, rvecs, tvecs, frame, best_marker)
                else:
                    draw_sword_cube(objtp, cal_mtx, cal_dist, rvecs, tvecs, frame, best_marker)

        # if not found_homography:
        #     window.config.update_homography(None)

        # Display the resulting frame
        cv2.imshow("webcam", frame)

        # Press Q on keyboard to exit
        if cv2.waitKey(1) == ord('q'):
            break
        # Close window when user clicks X
        if cv2.getWindowProperty("webcam", 0) == -1:
            break

    # if not window.is_closing:
    #     window.close()

    # When everything done, release the capture
    cap.release()
    # Close all windows
    cv2.destroyAllWindows()
