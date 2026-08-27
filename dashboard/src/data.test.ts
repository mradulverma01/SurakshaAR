import { describe, expect, it } from "vitest";
import { verifyCertificate } from "./data";

describe("verifyCertificate", () => {
  it("does not fabricate verification without a configured backend", async () => {
    await expect(verifyCertificate("cert-dead2026abcdef01")).resolves.toEqual({
      valid: false,
      certificateCode: "CERT-DEAD2026ABCDEF01",
    });
  });

  it("does not verify an unknown demonstration code", async () => {
    await expect(verifyCertificate("CERT-UNKNOWN0000000")).resolves.toEqual({
      valid: false,
      certificateCode: "CERT-UNKNOWN0000000",
    });
  });
});
