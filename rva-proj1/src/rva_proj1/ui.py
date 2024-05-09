
import numpy as np
import cv2

# Exception class for quitting the program
class Quit(Exception):
    pass

# Function for UI to choose the type of marker sword
def choose_marker_type():

    # Create a white image
    width, height = 1000, 600
    white_color = (255, 255, 255)
    white_image = np.full((height, width, 3), white_color, dtype=np.uint8)

    # Define center of the image for text
    x = (width - cv2.getTextSize("Choose the type of marker sword you'll be using", cv2.FONT_HERSHEY_SIMPLEX, 1,3)[0][0] )// 2
    y = height // 2

    # Write text
    cv2.putText(white_image, "Choose the type of marker sword you'll be using", (x, 60), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 0), 3)

    # Define center of the image for text
    x = (width - cv2.getTextSize("Press 'f' for flat sword", cv2.FONT_HERSHEY_SIMPLEX, 1,2)[0][0] )// 2

    # Write promp text
    cv2.putText(white_image, "Press 'f' for flat sword", (x, y), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 0), 2)
    cv2.putText(white_image, "Press 'c' for cube sword", (x, y + 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 0), 2)

    # Show image
    cv2.imshow("Choose Markers", white_image)

    # Wait for user input
    while True:
        key = cv2.waitKey(1)

        # Check if user pressed 'f' or 'c'
        if key == ord('f'):
            cv2.destroyAllWindows() # Close window
            return True
        if key == ord('c'):
            cv2.destroyAllWindows() # Close window
            return False
        # Check if user pressed 'q' or closed the window
        if key == ord('q'):
            raise Quit("User pressed 'q' key or closed the window") # Raise exception
        if cv2.getWindowProperty("Choose Markers",0) == -1:
            raise Quit("User pressed 'q' key or closed the window") # Raise exception


def calibrate_camera():

    # Create a white image
    width, height = 1000, 600
    white_color = (255, 255, 255)
    white_image = np.full((height, width, 3), white_color, dtype=np.uint8)

    # Define center of the image for text
    x1 = (width - cv2.getTextSize("The camera is not calibrated", cv2.FONT_HERSHEY_SIMPLEX, 1,2)[0][0] )// 2
    x2 = (width - cv2.getTextSize("Please keep your chessboard pattern in frame and wait, p", cv2.FONT_HERSHEY_SIMPLEX, 1,2)[0][0] )// 2
    y = height // 2

    # Write text
    cv2.putText(white_image, "The camera is not calibrated", (x1, y), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 0), 2)
    cv2.putText(white_image, "Please keep your chessboard pattern in frame and wait", (x2, y + 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 0), 2)

    # Show image
    cv2.imshow("Calibrate message", white_image)
