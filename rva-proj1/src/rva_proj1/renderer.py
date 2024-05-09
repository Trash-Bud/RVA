from pathlib import Path

import moderngl
from moderngl_window import geometry, WindowConfig
from moderngl_window.scene.camera import OrbitCamera
import numpy
from pyrr import Matrix44, Quaternion, Vector3


class Renderer(WindowConfig):
    gl_version = (3, 3)

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

        # Load the sword
        self.scene = self.load_scene(str(Path(__file__).parent.joinpath("models/minecraft_diamond_sword.glb")))
        self.original_matrix = self.scene.matrix

        # Set up the camera
        # An OrbitCamera can be rotated with the mouse, it can be switched to a regular Camera to have a fixed position
        self.camera = OrbitCamera(aspect_ratio=self.wnd.aspect_ratio, radius=0.5, near=0.1, far=1000.0)

        # Prepare the geometry, texture and shaders to display the camera feed
        self.quad = geometry.quad_2d(size=(2.0, 2.0))
        self.texture = self.ctx.texture(self.window_size, 3, data=numpy.zeros((self.window_size[0], self.window_size[1], 3), dtype=numpy.uint8))
        self.texture_program = self.ctx.program(
            vertex_shader="""
            #version 330
            in vec2 in_position;
            in vec2 in_texcoord_0;
            out vec2 uv;
            void main() {
                gl_Position = vec4(in_position, 0.9999999, 1.0);
                uv = in_texcoord_0;
            }
            """,
            fragment_shader="""
            #version 330
            uniform sampler2D tex;
            in vec2 uv;
            out vec4 f_color;
            void main() {
                f_color = texture(tex, uv);
            }
            """,
        )
        self.current_frame: numpy.ndarray | None = None
        self.frame_was_updated = False

        self.homography_adjustment = Matrix44.identity()
        self.homography_was_updated = False

    def render(self, time: float, frametime: float):
        self.ctx.enable_only(moderngl.DEPTH_TEST | moderngl.CULL_FACE)
        self.ctx.clear()

        # Display the camera frame as the background
        if self.current_frame is not None:
            if self.frame_was_updated:
                self.frame_was_updated = False
                self.texture.write(self.current_frame)

            self.texture.use()
            self.quad.render(self.texture_program)

        # Reposition the sword
        if self.homography_was_updated:
            self.scene.matrix = self.original_matrix * self.homography_adjustment

        # Draw the sword
        self.scene.draw(self.camera.projection.matrix, self.camera.matrix, time)

    def mouse_drag_event(self, x: int, y: int, dx, dy):
        self.camera.rot_state(dx, dy)

    def mouse_scroll_event(self, x_offset: float, y_offset: float):
        self.camera.zoom_state(y_offset)

    def resize(self, width: int, height: int):
        self.camera.projection.update(aspect_ratio=self.wnd.aspect_ratio)

    def update_frame(self, frame: numpy.ndarray):
        self.current_frame = frame
        self.frame_was_updated = True

    def update_homography(self, homography: numpy.ndarray | None):
        if homography is not None:
            # TODO: self.homography_adjustment = ???
            ...
        else:
            # keep it visible if no homography found for now,
            # TODO switch to the bottom version to hide the sword if no homography is found
            self.homography_adjustment = Matrix44.identity()
            # self.homography_adjustment = Matrix44.from_scale(Vector3([0, 0, 0]))

        self.homography_was_updated = True
