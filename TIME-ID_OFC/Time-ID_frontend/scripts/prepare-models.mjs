import { copyFile, mkdir } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = dirname(dirname(fileURLToPath(import.meta.url)));
const source = join(root, "node_modules", "@vladmandic", "face-api", "model");
const target = join(root, "public", "models");
await mkdir(target, { recursive: true });
for (const file of [
  "tiny_face_detector_model-weights_manifest.json", "tiny_face_detector_model.bin",
  "face_landmark_68_tiny_model-weights_manifest.json", "face_landmark_68_tiny_model.bin",
  "face_recognition_model-weights_manifest.json", "face_recognition_model.bin",
]) await copyFile(join(source, file), join(target, file));
