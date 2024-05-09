from pathlib import Path

import cv2 as cv
import numpy as np

GAUSSIAN_KERNEL_SIZE = 13
MAX_AREA_PERCENTAGE = 0.4
MIN_AREA_PERCENTAGE = 0.01
APPROXIMATION_ACCURACY = 0.05
MAX_WIDTH_RATIO = 0.2
MAX_HEIGHT_RATIO = 0.2
MAX_HEIGHT_WIDTH_RATIO = 1.25
MAX_XOR_PERCENTAGE = 40

#open sword markers
MARKER_DIR = Path(__file__).parent.joinpath("markers")
FRONT_SWORD_MARKER = cv.imread(str(MARKER_DIR.joinpath("front_sword.png")), cv.IMREAD_GRAYSCALE)
BACK_SWORD_MARKER = cv.imread(str(MARKER_DIR.joinpath("back_sword.png")), cv.IMREAD_GRAYSCALE)
LEFT_SWORD_MARKER = cv.imread(str(MARKER_DIR.joinpath("left_sword.png")), cv.IMREAD_GRAYSCALE)
RIGHT_SWORD_MARKER = cv.imread(str(MARKER_DIR.joinpath("right_sword.png")), cv.IMREAD_GRAYSCALE)
TOP_SWORD_MARKER = cv.imread(str(MARKER_DIR.joinpath("top_sword.png")), cv.IMREAD_GRAYSCALE)

# sword markers size
MARKER_SIZE = len(FRONT_SWORD_MARKER)

# sword makers indexes
FRONT_SWORD = 0
BACK_SWORD = 1
LEFT_SWORD = 2
RIGHT_SWORD = 3
TOP_SWORD = 4

# create list with all the sword markers
MARKER_LIST = [FRONT_SWORD_MARKER, BACK_SWORD_MARKER, LEFT_SWORD_MARKER, RIGHT_SWORD_MARKER, TOP_SWORD_MARKER]

ROTATION_90 = 0
ROTATION_180 = 1
ROTATION_270 = 2
ROTATION_0 = 3

# filter the contours found in a frame to return the contour of the marker and its warped perspective
def filter_contours(contours, threshold):
    valid_contour = []
    biggest_area = 0
    current_box = []
    return_contour = []

    # calculates max and min area acceptable
    img_area = threshold.shape[0] * threshold.shape[1]
    max_area = MAX_AREA_PERCENTAGE * img_area
    min_area = MIN_AREA_PERCENTAGE * img_area

    for contour in contours:
        # gets area of contour
        area = cv.contourArea(contour)

        # filters per contour area
        if area > max_area or area < min_area:
            continue
        
        # get the approximate shape of the contour
        epsilon = APPROXIMATION_ACCURACY * cv.arcLength(contour, True)
        approx = cv.approxPolyDP(contour, epsilon, True)

        # only accepts shapes with 4 sides
        if len(approx) != 4:
            continue
        
        # gets box points
        box = np.squeeze(approx).astype(np.float32)
        p0, p1, p2, p3 = box[2], box[1], box[0], box[3]

        # gets sides lengths
        width_03 = np.sqrt(((p0[0] - p3[0]) ** 2) + ((p0[1] - p3[1]) ** 2))
        width_12 = np.sqrt(((p1[0] - p2[0]) ** 2) + ((p1[1] - p2[1]) ** 2))

        height_01 = np.sqrt(((p0[0] - p1[0]) ** 2) + ((p0[1] - p1[1]) ** 2))
        height_23 = np.sqrt(((p2[0] - p3[0]) ** 2) + ((p2[1] - p3[1]) ** 2))

        max_width = max(int(width_03), int(width_12))
        max_height = max(int(height_01), int(height_23))

        # evaluates if all sides are about the same size
        if (
            abs(width_03 - width_12) > MAX_WIDTH_RATIO * max_width
            or abs(height_01 - height_23) > MAX_HEIGHT_RATIO * max_height
            or abs(max_height - max_width) > MAX_HEIGHT_WIDTH_RATIO * min(max_height, max_width)
        ):
            continue
        
        # records current biggest square-like contour
        if area > biggest_area:
            biggest_area = area
            current_box = box
            valid_contour = contour

    # if there are valid contours, compute the warped perspective of the marker, if not, return empty list
    if len(valid_contour) != 0:
        output_height = MARKER_SIZE
        output_width = MARKER_SIZE

        # get the points to warp the perspective of the marker
        input_pts = np.float32([current_box[2], current_box[1], current_box[0], current_box[3]])
        output_pts = np.float32(
            [
                [0, 0],
                [0, output_height - 1],
                [output_width - 1, output_height - 1],
                [output_width - 1, 0]
            ]
        )

        # get warped marker
        transformation_matrix = cv.getPerspectiveTransform(input_pts, output_pts)
        warped_marker_image = cv.warpPerspective(
            threshold, transformation_matrix,
            (output_width, output_height),
            flags=cv.INTER_LINEAR
        )

        # append values to return
        return_contour.append((valid_contour, warped_marker_image, current_box))

    return return_contour

# Find the contour with the best match to the template
def find_best_match(output, use_xor_filter=True):
    min_result = 99999999
    best_marker, rotation = None, None

    # gets warped perspective of the marker
    _, marker_image, _ = output[0]

    # goes through every marker to see which one is the best match
    for m in range(len(MARKER_LIST)):
        # to identify the rotation of the marker
        for i in range(4):
            # rotates marker 90 degrees every loop
            marker_image = cv.rotate(marker_image, cv.ROTATE_90_CLOCKWISE)

            # uses this filter to better the performance
            if use_xor_filter:
                # if the percentage of the 2 images that are the same is below a certain value it does not do template matching
                xor = cv.bitwise_xor(marker_image, MARKER_LIST[m]).sum()
                xor_percentage = xor / (marker_image.shape[0] * marker_image.shape[1])
                if xor_percentage > MAX_XOR_PERCENTAGE:
                    continue
            
            # gets result of template matching
            result = cv.matchTemplate(
                marker_image, MARKER_LIST[m], cv.TM_SQDIFF_NORMED
            )

            # saves current best result
            if result < min_result:
                min_result = result
                best_marker = m
                rotation = i

    # returns the best_marker and the rotation of the marker in the image
    return best_marker, rotation, min_result

# finds the homography between the template marker and the marker in the box
def find_homography(box, rotation):
    marker_height = MARKER_SIZE
    marker_width = MARKER_SIZE

    # gets marker points
    marker_pts = np.float32(
            [
                [0, 0],
                [0, marker_height - 1],
                [marker_width - 1,  marker_height - 1],
                [marker_width - 1, 0]
            ]
        ).reshape(-1,1,2)

    # changes box points dependent on the marker's rotation and computes the homography
    if rotation == ROTATION_90:
        box_pts = np.float32([box[1], box[0], box[3], box[2]]).reshape(-1,1,2)
        return cv.findHomography(marker_pts, box_pts)
    if rotation == ROTATION_180:
        box_pts = np.float32([box[0], box[3], box[2], box[1]]).reshape(-1,1,2)
        return cv.findHomography(marker_pts, box_pts)
    if rotation == ROTATION_270:
        box_pts = np.float32([box[3], box[2], box[1], box[0]]).reshape(-1,1,2)
        return cv.findHomography(marker_pts, box_pts)
    if rotation == ROTATION_0:
        box_pts = np.float32([box[2], box[1], box[0], box[3]]).reshape(-1,1,2)
        return cv.findHomography(marker_pts, box_pts)

# Define object points for each marker rotation
def define_obj_pts(rotation):
    if rotation == ROTATION_90:
        return np.array([[0,0,0],[0,1,0],[1,1,0],[1,0,0]])
    if rotation == ROTATION_180:
        return np.array([[0,1,0],[1,1,0],[1,0,0],[0,0,0]])
    if rotation == ROTATION_270:
        return np.array([[1,1,0],[1,0,0],[0,0,0],[0,1,0]])
    if rotation == ROTATION_0:
        return np.array([[1,0,0],[0,0,0],[0,1,0],[1,1,0]])
