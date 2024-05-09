
import cv2
import numpy

from .marker import BACK_SWORD, FRONT_SWORD, LEFT_SWORD, RIGHT_SWORD, TOP_SWORD

# sword base points
BASE_SWORD_POINTS = numpy.float32([[-2.25,-0.25,-0.25], [-2.25,0.25,-0.25], [-2.25,0.25,0.25], [-2.25,-0.25,0.25], [-1.25,-0.25,-0.25], [-1.25,0.25,-0.25], [-1.25,0.25,0.25], [-1.25,-0.25,0.25]]).reshape(-1,3)
# sword hilt points
HILT_SWORD_POINTS = numpy.float32([[-0.75,-0.75,-0.75], [-0.75,0.75,-0.75], [-1.25,0.75,-0.75], [-1.25,-0.75,-0.75], [-0.75,-0.75,0.75], [-0.75,0.75,0.75], [-1.25,0.75,0.75], [-1.25,-0.75,0.75]]).reshape(-1,3)
# sword blade points
BLADE_SWORD_POINTS = numpy.float32([[2.25,0,0], [-0.75,0.5,0.5], [-0.75,0.5,-0.5], [-0.75,-0.5,-0.5], [-0.75,-0.5,0.5]]).reshape(-1,3)

# sword components indexes
BASE_SWORD = 0
HILT_SWORD = 1
BLADE_SWORD = 2

# list with all the sword components
SWORD = [BASE_SWORD_POINTS,HILT_SWORD_POINTS,BLADE_SWORD_POINTS]

# draw parallelepiped for sword base and hilt
def drawBoxes(img, imgpts):

    imgpts = numpy.int32(imgpts).reshape(-1,2)

    # draw base
    img = cv2.drawContours(img, [imgpts[:4]],-1,(0,0,255),3)

    # draw pillars
    img = cv2.line(img, tuple(imgpts[0]), tuple(imgpts[4]),(255,0,0),3)
    img = cv2.line(img, tuple(imgpts[1]), tuple(imgpts[5]),(255,255,0),3)
    img = cv2.line(img, tuple(imgpts[2]), tuple(imgpts[6]),(255,0,255),3)
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[7]),(0,255,0),3)

    # draw top
    img = cv2.drawContours(img, [imgpts[4:]],-1,(0,0,255),3)
    return img

# draw pyramid for sword blade
def drawPyramid(img, imgpts):
    imgpts = numpy.int32(imgpts).reshape(-1,2)

    # draw highest point
    tip = tuple(imgpts[0])

    #draw other points
    for i in range(4):
        img = cv2.line(img, tip, tuple(imgpts[i + 1]), (0, 255, 0), 3)

    img = cv2.drawContours(img, [imgpts[1:]],-1,(0,0,255),3)
    return img


# get rotation matrix for each marker
def get_rotation(best_marker):
    # z rotation because our sword is tilted in the Z axis
    rotationMatrixZ = numpy.array([
        [numpy.cos(numpy.radians(-90)), -numpy.sin(numpy.radians(-90)), 0],
        [numpy.sin(numpy.radians(-90)), numpy.cos(numpy.radians(-90)), 0],
        [0, 0, 1]
        ], dtype=numpy.float32)

    # get rotation matrix for each marker
    if best_marker == FRONT_SWORD:
        rotationMatrix = numpy.array([
            [numpy.cos(0), 0, numpy.sin(0)],
            [0, 1, 0],
            [-numpy.sin(0), 0, numpy.cos(0)]
        ])
    if best_marker == BACK_SWORD:
        rotationMatrix = numpy.array([
            [numpy.cos(numpy.pi), 0, numpy.sin(numpy.pi)],
            [0, 1, 0],
            [-numpy.sin(numpy.pi), 0, numpy.cos(numpy.pi)]
        ])
    if best_marker == LEFT_SWORD:
        rotationMatrix = numpy.array([
            [numpy.cos(numpy.pi / 2), 0, numpy.sin(numpy.pi / 2)],
            [0, 1, 0],
            [-numpy.sin(numpy.pi / 2), 0, numpy.cos(numpy.pi / 2)]
        ])
    if best_marker == RIGHT_SWORD:
        rotationMatrix = numpy.array([
            [numpy.cos(-numpy.pi / 2), 0, numpy.sin(-numpy.pi / 2)],
            [0, 1, 0],
            [-numpy.sin(-numpy.pi / 2), 0, numpy.cos(-numpy.pi / 2)]
        ])
    if best_marker == TOP_SWORD:
        rotationMatrix = numpy.array([
            [1, 0, 0],
            [0, numpy.cos(numpy.pi / 2), -numpy.sin(numpy.pi / 2)],
            [0, numpy.sin(numpy.pi / 2), numpy.cos(numpy.pi / 2)]
        ])
        # this marker is on top and needs to rotate the other way around
        rotationMatrixZ = numpy.array([
            [numpy.cos(numpy.radians(90)), -numpy.sin(numpy.radians(90)), 0],
            [numpy.sin(numpy.radians(90)), numpy.cos(numpy.radians(90)), 0],
            [0, 0, 1]
            ], dtype=numpy.float32)

    # combine rotation matrices
    rotationMatrix = numpy.dot(rotationMatrixZ, rotationMatrix)
    return rotationMatrix

# Calculate center of object points to align sword
def calculate_object_points_center(objtp, z_value,best_marker):
    # calculate center of object points
    objtp_center = numpy.mean(objtp, axis=0)
    # set z value so that sword is not glued to the marker
    # and instead the marker is in the middle of the sword
    objtp_center[2] = z_value

    # in the top marker since we rotate -90 degrees in Z we also need to rotate the center points of the object
    if best_marker == TOP_SWORD:
        # rotation matrix for -90 degrees in Z
        rotation_matrix = numpy.array([[0, -1, 0],
                                [1, 0, 0],
                                [0, 0, 1]])
        # apply rotation
        objtp_center = numpy.dot(rotation_matrix, objtp_center)

    return objtp_center

# apply transformations for each section of the sword
def apply_transformations(objtp_center,rotationMatrix,mtx, dist, rvecs, tvecs , frame, best_marker):

    for i in range(3):
        # get points for each section of the sword
        axisBoxes = SWORD[i] + numpy.array([1, 0.5, 0], dtype=numpy.float32)

        # rotate sword
        axisBoxes = numpy.dot(axisBoxes, rotationMatrix)
        # get sword center
        axisBoxesCenter = numpy.mean(axisBoxes, axis=0)
        # calculate translation to align sword by subtracting the center of the sword from the center of the object points
        translation = objtp_center - axisBoxesCenter

        if (best_marker == TOP_SWORD):
            # for top marker the axis rotate -90 degrees in the Z axis so we have to stop the x axis translation instead
            translation[0] = 0
        else:
            # remove y axis translation because the sword is always on the same height
            translation[1] = 0

        # apply translation
        axisBoxes += translation

        # project points
        imgpts, _ = cv2.projectPoints(axisBoxes, rvecs, tvecs, mtx, dist)

        # draw sword
        if i == BLADE_SWORD:
            drawPyramid(frame, imgpts)
        else:
            drawBoxes(frame, imgpts)


# draw sword for flat configuration
def draw_sword_flat(objtp, mtx, dist, rvecs, tvecs , frame, best_marker):

    # calculate center of object points to align sword
    objtp_center = calculate_object_points_center(objtp, -0.5,best_marker)

    # calculate rotation matrix for each marker
    rotationMatrix = get_rotation(best_marker)

    # apply transformations and draw sword
    apply_transformations(objtp_center,rotationMatrix,mtx, dist, rvecs, tvecs , frame, best_marker)


# draw sword for cube configuration
def draw_sword_cube(objtp, mtx, dist, rvecs, tvecs , frame, best_marker):

    # calculate center of object points to align sword
    objtp_center = calculate_object_points_center(objtp, -1.5, best_marker)

    # calculate rotation matrix for each marker
    rotationMatrix = get_rotation(best_marker)

    # apply transformations and draw sword
    apply_transformations(objtp_center,rotationMatrix,mtx, dist, rvecs, tvecs , frame, best_marker)

